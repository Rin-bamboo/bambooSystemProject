namespace common
{
    /// <summary>
    /// 各データの検証を行います。
    /// </summary>
    public class Validate
    {
        /// <summary>
        /// 渡された空かどうかを判断します
        /// </summary>
        /// <param name="value"></param>
        /// <returns>nullや空白の場合はTrueそれ以外はFalseを返却します</returns>
        public bool IsEmpty(object value)
        {
            if (value == null) return true;
            return false;
        }
        /// <summary>
        /// 渡された値が空白じゃないかを判断します。
        /// </summary>
        /// <param name="value"></param>
        /// <returns>nullや空白の場合はFalse、値が入っている場合はTrueを返却します</returns>
        public bool IsNotEmpty(object value)
        {
            return !IsEmpty(value);
        }
        /// <summary>
        /// 数値かどうかを判断します
        /// </summary>
        /// <param name="value"></param>
        /// <returns>数値の場合はTrue そうじゃない場合はFalseを返却します</returns>
        public bool IsNumeric(object value)
        {
            if (value == null) return false;
            if ((value is string) &&
                !(double.TryParse(value.ToString(), out _)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// データが対象のオブジェクトのTypeと一致しているかcheck
        /// </summary>
        /// <param name="value">チェックしたいデータ</param>
        /// <param name="Type">チャックしたいオブジェクトType Class等</param>
        /// <returns>一致している場合は Ture それ以外はFalse</returns>
        public bool IsObjectCheck(Object value, Object Type)
        {
            if (value.GetType() == Type.GetType())
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 対象二つのデータが一致しているか確認します
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public bool IsEqual(Object value1, Object value2)
        {
            if (value1.Equals(value2))
            {
                return true;
            }
            return true;
        }


    }
}
