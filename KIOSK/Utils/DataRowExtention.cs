using System.Data;

namespace KIOSK.Utils
{
    public static class DataRowExtensions
    {
        // TODO: Exception Log 필요

        /// <summary>
        /// 문자열 컬럼 안전 조회
        /// </summary>
        public static string GetString(this DataRow row, string columnName, string defaultValue = "")
        {
            if (row == null) return defaultValue;

            if (!row.Table.Columns.Contains(columnName))
                return defaultValue;

            var value = row[columnName];
            return value == DBNull.Value ? defaultValue : value.ToString()!;
        }

        /// <summary>
        /// 제네릭 타입 안전 조회
        /// </summary>
        public static T Get<T>(this DataRow row, string columnName, T defaultValue = default!)
        {
            if (row == null) return defaultValue;

            if (!row.Table.Columns.Contains(columnName))
                return defaultValue;

            var value = row[columnName];
            if (value == DBNull.Value) return defaultValue;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                // 변환 실패 시 기본값 반환 → 프로그램 중단 방지
                return defaultValue;
            }
        }
    }
}
