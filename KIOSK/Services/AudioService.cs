using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Services
{
    public interface IAudioService : IDisposable
    {
        // TODO: 미리 Key-Value로 지정 기능 추가 (Value = 단순 음원 경로 또는 메모리 저장)

        /// <summary>파일 경로로 재생</summary>
        void Play(string filePath);

        /// <summary>모든 재생 중지</summary>
        void StopAll();

        /// <summary>0.0 ~ 1.0 전역 볼륨</summary>
        float Volume { get; set; }
    }

    public class AudioService : IAudioService
    {
        private readonly ILoggingService _logging;
        private readonly ConcurrentDictionary<string, CachedSound> _cache = new();
        private readonly object _lock = new();
        private WaveOutEvent _outputDevice;
        private MixingSampleProvider _mixer;
        private VolumeSampleProvider _masterVolume;
        private float _volume = 1.0f;

        // 현재 믹서에 추가된 (재생 중인) 입력. 새 재생 시 이 입력을 제거합니다.
        private ISampleProvider? _currentInput;

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = Math.Clamp(value, 0f, 1f);
                lock (_lock)
                {
                    if (_masterVolume != null)
                        _masterVolume.Volume = _volume;
                }
            }
        }

        public AudioService(ILoggingService logging, int sampleRate = 44100, int channels = 2)
        {
            _logging = logging;
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
            _mixer = new MixingSampleProvider(waveFormat) { ReadFully = true };
            _masterVolume = new VolumeSampleProvider(_mixer) { Volume = _volume };

            _outputDevice = new WaveOutEvent();
            _outputDevice.Init(_masterVolume);
            _outputDevice.Play();
        }

        private CachedSound GetOrLoadCachedSound(string filePath)
        {
            return _cache.GetOrAdd(filePath, path =>
            {
                try
                {
                    var target = _mixer.WaveFormat;
                    return new CachedSound(path, target);
                }
                catch (Exception ex)
                {
                    
                    Debug.WriteLine($"CachedSound 생성 실패: {ex}");
                    throw;
                }
            });
        }

        private void TryRemoveCurrentInputOrReset()
        {
            if (_currentInput == null) return;

            try
            {
                _mixer.RemoveMixerInput(_currentInput);
                _currentInput = null;

                return;
            }
            catch (Exception ex)
            {
                _logging.Error(ex, "RemoveMixerInput Exception");
                //Debug.WriteLine($"RemoveMixerInput 호출 중 예외: {ex}");
            }

            // RemoveMixerInput 사용 불가 또는 실패 시 안전하게 리셋
            //StopAll();
        }

        public void Play(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;

            CachedSound cs;
            try
            {
                cs = GetOrLoadCachedSound(filePath);
            }
            catch
            {
                return;
            }

            var provider = new CachedSoundSampleProvider(cs);

            lock (_lock)
            {
                // 1) 기존 재생 제거
                TryRemoveCurrentInputOrReset();

                // 2) 새로운 입력 추가 및 현재 입력으로 지정
                _mixer.AddMixerInput(provider);
                _currentInput = provider;
            }

            _logging.Info($"Play Audio ({Path.GetFileName(filePath)})");
        }

        /// <summary>
        /// 즉시 모든 재생을 중지. mixer/output을 재생성하여 완전 초기화.
        /// </summary>
        public void StopAll()
        {
            lock (_lock)
            {
                try
                {
                    _outputDevice?.Stop();
                    _outputDevice?.Dispose();

                    var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(_mixer.WaveFormat.SampleRate, _mixer.WaveFormat.Channels);
                    _mixer = new MixingSampleProvider(waveFormat) { ReadFully = true };
                    _masterVolume = new VolumeSampleProvider(_mixer) { Volume = _volume };

                    _outputDevice = new WaveOutEvent();
                    _outputDevice.Init(_masterVolume);
                    _outputDevice.Play();

                    _currentInput = null;

                    _logging.Info("Stop All Audio");
                }
                catch (Exception ex)
                {
                    _logging.Error(ex, "StopAll Exception");
                    //Debug.WriteLine($"StopAll 예외: {ex}");
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                try
                {
                    _outputDevice?.Stop();
                    _outputDevice?.Dispose();
                    _outputDevice = null;
                }
                catch { }
            }
        }
    }

    internal class CachedSound
    {
        public float[] AudioData { get; }
        public WaveFormat WaveFormat { get; }

        public CachedSound(string filePath, WaveFormat targetFormat)
        {
            using var reader = new AudioFileReader(filePath);
            ISampleProvider provider = reader.ToSampleProvider();

            // 샘플레이트 다르면 리샘플링
            if (provider.WaveFormat.SampleRate != targetFormat.SampleRate)
            {
                provider = new WdlResamplingSampleProvider(provider, targetFormat.SampleRate);
            }

            // 채널 다르면 변환 (mono, stereo)
            if (provider.WaveFormat.Channels != targetFormat.Channels)
            {
                if (provider.WaveFormat.Channels == 1 && targetFormat.Channels == 2)
                {
                    provider = new MonoToStereoSampleProvider(provider);
                }
                else if (provider.WaveFormat.Channels == 2 && targetFormat.Channels == 1)
                {
                    provider = new StereoToMonoSampleProvider(provider);
                }
                else
                {
                    throw new NotSupportedException("지원하지 않는 채널 구성입니다.");
                }
            }

            WaveFormat = targetFormat;

            var whole = new List<float>();
            var buffer = new float[targetFormat.SampleRate * targetFormat.Channels];
            int read;
            while ((read = provider.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < read; i++) whole.Add(buffer[i]);
            }
            AudioData = whole.ToArray();
        }
    }

    internal class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound cached;
        private long position;

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            cached = cachedSound;
            position = 0;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var available = cached.AudioData.Length - position;
            if (available <= 0) return 0;
            var toCopy = (int)Math.Min(available, count);
            Array.Copy(cached.AudioData, position, buffer, offset, toCopy);
            position += toCopy;
            return toCopy;
        }

        public WaveFormat WaveFormat => cached.WaveFormat;
    }
}
