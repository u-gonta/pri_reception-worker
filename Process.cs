using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Process
{
    /// <summary>
    /// 処理の基礎クラス
    /// </summary>
    public class Basis : IDisposable
    {
        /// <summary>
        /// ロックするためのインスタンス
        /// </summary>
        protected readonly object _asyncMain = new object();

        /// <summary>
        /// メイン関数のスレッド
        /// </summary>
        private Thread _thread = null;

        /// <summary>
        /// メイン関数への要求
        /// </summary>
        protected readonly EventWaitHandle _request = new EventWaitHandle(true, EventResetMode.ManualReset);

        /// <summary>
        /// ロックするためのインスタンス
        /// </summary>
        protected readonly object _asyncStop = new object();

        /// <summary>
        /// メイン関数の停止要求
        /// </summary>
        private bool _requestStop = false;
        protected bool GetRequestStop()
        {
            bool ret = false;

            // 排他制御
            lock (_asyncStop)
            {
                ret = _requestStop;
            }

            return ret;
        }
        protected void SetRequestStop(bool value)
        {
            // 排他制御
            lock (_asyncStop)
            {
                _requestStop = value;
            }
        }

        /// <summary>
        /// ロックするためのインスタンス
        /// </summary>
        private readonly object _asyncRunning = new object();

        /// <summary>
        /// メイン関数の動作中フラグ
        /// </summary>
        private bool _running = false;
        protected bool GetRunning()
        {
            bool ret = false;

            // 排他制御
            lock (_asyncRunning)
            {
                ret = _running;
            }

            return ret;
        }
        protected void SetRunning(bool value)
        {
            // 排他制御
            lock (_asyncRunning)
            {
                _running = value;
            }
        }

        /// <summary>
        /// 名称 ※プロパティ
        /// </summary>
        protected string Name { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">名称</param>
        public Basis(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 破棄 ※派生先のクラスで記述
        /// </summary>
        public virtual void Dispose()
        {
            _request.Dispose();
        }

        /// <summary>
        /// スレッド開始
        /// </summary>
        public void Start()
        {
            // スレッド停止
            Stop();

            // メイン関数の停止要求を解除
            SetRequestStop(false);

            // 排他制御
            lock (_asyncMain)
            {
                // メイン関数を構築
                if (_thread == null)
                {
                    _thread = new Thread(new ThreadStart(this.Main));
                }

                // メイン関数をスタート
                _thread.Start();
            }
        }

        /// <summary>
        /// スレッド停止
        /// </summary>
        public void Stop()
        {
            // キャンセル
            Cancel();

            // メイン関数の停止要求を更新
            SetRequestStop(true);

            Task task = null;

            // 要求
            if (_request.Set() == false)
            {
                throw new Exception("Process::Stop()" + Output.Basis.ConstSeparator + "要求エラー");
            }

            // 排他制御
            lock (_asyncMain)
            {
                try
                {
                    if (_thread != null)
                    {
                        // メイン関数の停止待ち
                        task = Task.Factory.StartNew(() => _thread.Join());
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            if (task != null)
            {
                // 待ち
                task.Wait();
            }

            // 排他制御
            lock (_asyncMain)
            {
                // 破棄
                if (_thread != null)
                {
                    _thread = null;
                }
            }
        }

        /// <summary>
        /// 終了 ※派生先のクラスで記述
        /// </summary>
        protected virtual void Finish()
        {

        }

        /// <summary>
        /// キャンセル ※派生先のクラスで記述
        /// </summary>
        protected virtual void Cancel()
        {

        }

        /// <summary>
        /// 行動 ※派生先のクラスで記述
        /// <returns>タイムアウト[ms]</returns>
        /// </summary>
        protected virtual int Action()
        {
            return 1000;
        }

        /// <summary>
        /// メイン関数
        /// </summary>
        protected void Main()
        {
            bool run;
            int timeout;

            run = true;
            timeout = 1000;

            while (run)
            {
                try
                {
                    bool ret;

                    // 要求待ち
                    ret = _request.WaitOne(timeout);

                    // 要求を解除
                    if (_request.Reset() == false)
                    {
                        throw new Exception("Process::Main()" + Output.Basis.ConstSeparator + "要求の解除エラー");
                    }

                    // 行動
                    timeout = Action();

                    // メイン関数の停止要求を確認
                    if (GetRequestStop())
                    {
                        // メイン関数の動作中フラグを確認
                        if (GetRunning())
                        {
                            timeout = 0;
                            continue;
                        }

                        // 終了
                        Finish();
                        run = false;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    break;
                }
            }
        }
    }
}
