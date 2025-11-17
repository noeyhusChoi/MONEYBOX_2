using KIOSK.Utils;
using System.Data;

namespace KIOSK.Services.DataBase
{
    public readonly record struct ApiConfigField(string ServerName, string ServerUrl, int TimeoutSeconds);

    public sealed class ApiConfigFieldService : BaseFieldService<ApiConfigField>
    {
        public ApiConfigFieldService(IDatabaseService db) : base(db) { }

        // SERVER_NAME 기준 조회용 캐시
        private readonly Dictionary<string, ApiConfigField> _byName = new(StringComparer.OrdinalIgnoreCase);


        // 사용할 프로시저 이름
        protected override string ProcedureName => "sp_get_server_info";

        // DataRow -> 매핑
        protected override ApiConfigField MapRow(DataRow row)
        {
            return new ApiConfigField()
            {
                ServerName = row.Get<string>("SERVER_NAME"),
                ServerUrl = row.Get<string>("SERVER_URL"),
                TimeoutSeconds = row.Get<int>("TIMEOUT_SECONDS")
            };
        }

        // 전체 필드
        public IReadOnlyList<ApiConfigField> GetAllFields() => GetAll();

        // 로드 후 Dictionary 재구성
        protected override void OnLoaded(IReadOnlyList<ApiConfigField> items)
        {
            _byName.Clear();
            foreach (var item in items)
            {
                _byName[item.ServerName] = item;
            }
        }

        public bool TryGet(string serverName, out ApiConfigField field)
            => _byName.TryGetValue(serverName, out field);

        public ApiConfigField GetRequired(string serverName)
            => _byName.TryGetValue(serverName, out var field)
                ? field
                : throw new InvalidOperationException(
                    $"sp_get_server_info 에 SERVER_NAME='{serverName}' 레코드가 없습니다.");
    }
}