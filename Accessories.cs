using System.Linq;

namespace Accessories
{
    /// <summary>
    /// ツールクラス
    /// </summary>
    public class Tool
    {
        /// <summary>
        /// ディレクトリパスを補正
        /// </summary>
        /// <param name="value">ディレクトリパス</param>
        /// <returns></returns>
        public static string CorrectionDirectory(string value)
        {
            string ret = "";

            do
            {
                // 文字の有無を確認
                if (string.IsNullOrEmpty(value))
                {
                    break;
                }

                // 最後に\が付いているか確認
                if (value.Last() != '\\')
                {
                    value += '\\';
                }
            } while (false);

            ret = value;

            return ret;
        }
    }
}
