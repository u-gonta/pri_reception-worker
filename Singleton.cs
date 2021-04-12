namespace Singleton
{
    /// <summary>
    /// シングルトンの基礎クラス ※テンプレート
    /// ★デストラクタが呼ばれない場合があるので破棄が必要な場合は呼ぶ必要あり
    /// </summary>
    public class Basis<T> where T : class, new()
    {
        /// <summary>
        /// インスタンス変数への代入が完了するまで、アクセスできなくなるジェネリックなインスタンス
        /// </summary>
        private static volatile T _instance;

        /// <summary>
        /// インスタンス変数をロックするためのインスタンス
        /// </summary>
        protected static object _async = new object();

        /// <summary>
        /// ジェネリックなインスタンス
        /// </summary>
        public static T Instance
        {
            get
            {
                // ダブルチェック ロッキング アプローチ
                if (_instance == null)
                {
                    // syncインスタンスをロックし、この型そのものをロックしないことで、デッドロックの発生を回避
                    lock (_async)
                    {
                        // インスタンスを確認
                        if (_instance == null)
                        {
                            // インスタンスを作成
                            _instance = new T();
                        }
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected Basis()
        {

        }
    }
}
