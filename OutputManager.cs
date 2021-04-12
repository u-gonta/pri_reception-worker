using System;

namespace Output
{
    /// <summary>
    /// 出力クラスを管理するクラス
    /// </summary>
    public class Manager : Singleton.Basis<Manager>, IDisposable
    {
        /// <summary>
        /// 出力クラスをロックするためのインスタンス
        /// </summary>
        private readonly object _asyncOutput = new object();

        /// <summary>
        /// 出力クラス
        /// </summary>
        private Basis _output = new Basis();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Manager()
        {

        }

        /// <summary>
        /// 破棄
        /// </summary>
        public void Dispose()
        {
            lock (_asyncOutput)
            {
                do
                {
                    // 出力クラスを確認
                    if (_output == null)
                    {
                        // 出力クラスなし ⇒ 処理をしない
                        break;
                    }

                    // 破棄
                    _output.Dispose();
                } while (false);
            }
        }

        /// <summary>
        /// 開始
        /// </summary>
        /// <param name="setting">設定クラス</param>
        public void Start(Setting setting)
        {
            lock (_asyncOutput)
            {
                do
                {
                    // 出力クラスを確認
                    if (_output == null)
                    {
                        // 出力クラスなし ⇒ 処理をしない
                        break;
                    }

                    // 更新
                    _output.SetSetting(setting);

                    // 開始
                    _output.Start();
                } while (false);
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            lock (_asyncOutput)
            {
                do
                {
                    // 出力クラスを確認
                    if (_output == null)
                    {
                        // 出力クラスなし ⇒ 処理をしない
                        break;
                    }

                    // 停止
                    _output.Stop();
                } while (false);
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="setting">設定クラス</param>
        public void Update(Setting setting)
        {
            lock (_asyncOutput)
            {
                do
                {
                    // 出力クラスを確認
                    if (_output == null)
                    {
                        // 出力クラスなし ⇒ 処理をしない
                        break;
                    }

                    // 更新
                    _output.SetSetting(setting);
                } while (false);
            }
        }

        /// <summary>
        /// バッファデータを追加
        /// </summary>
        /// <param name="data">データクラス</param>
        public void Enqueue(Data data)
        {
            lock (_asyncOutput)
            {
                do
                {
                    // 出力クラスを確認
                    if (_output == null)
                    {
                        // 出力クラスなし ⇒ 処理をしない
                        break;
                    }

                    // バッファデータを追加
                    _output.Enqueue(data);
                } while (false);
            }
        }
    }
}
