using KIOSK.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Services.DataBase
{
    public abstract class BaseFieldService<T>
    {
        private readonly IDatabaseService _db;

        // 기본 캐시는 리스트
        private volatile List<T> _items = new();

        protected BaseFieldService(IDatabaseService db)
        {
            _db = db;
        }

        /// <summary>
        /// 호출할 Stored Procedure 이름 (각 자식 클래스에서 구현)
        /// </summary>
        protected abstract string ProcedureName { get; }

        /// <summary>
        /// DataRow -> T 매핑 (각 자식 클래스에서 구현)
        /// </summary>
        protected abstract T MapRow(DataRow row);

        /// <summary>
        /// 로드 완료 후 후처리 (키 설정 가능 DB만 쓸 수 있을 예정..)
        /// </summary>
        protected virtual void OnLoaded(IReadOnlyList<T> items) { }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            await LoadAsync(ct).ConfigureAwait(false);
        }

        public IReadOnlyList<T> GetAll() => _items;

        protected async Task LoadAsync(CancellationToken ct)
        {
            try
            {
                var ds = await _db.QueryAsync<DataSet>(
                    ProcedureName,
                    type: CommandType.StoredProcedure,
                    ct: ct
                ).ConfigureAwait(false);

                if (ds.Tables.Count < 1)
                    return;

                var table = ds.Tables[0];
                var list = new List<T>(table.Rows.Count);

                foreach (DataRow row in table.Rows)
                {
                    var item = MapRow(row);
                    list.Add(item);
                }

                // 리스트 스냅샷 교체
                _items = list;

                // 자식 클래스에서 Dictionary 캐시 등 추가 구성 가능
                OnLoaded(list);
            }
            catch (Exception ex)
            {
                // TODO: 로깅 서비스 주입해서 사용해도 됨
                Console.WriteLine(ex);
            }
        }
    }
}
