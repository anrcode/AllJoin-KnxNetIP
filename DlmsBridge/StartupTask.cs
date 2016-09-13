using System;
using Windows.ApplicationModel.Background;
using BridgeRT;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace DlmsBridge
{
    public sealed class StartupTask : IBackgroundTask
    {
        private DsbBridge dsbBridge;
        private BackgroundTaskDeferral deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {

            IAdapter adapter = null;
            deferral = taskInstance.GetDeferral();

            try
            {
                adapter = new DlmsAdapter.WrappedAdapter();
                dsbBridge = new DsbBridge(adapter);

                var initResult = dsbBridge.Initialize();
                if (initResult != 0)
                {
                    throw new Exception("DSB Bridge initialization failed!");
                }
            }
            catch (Exception ex)
            {
                dsbBridge?.Shutdown();
                adapter?.Shutdown();

                throw;
            }
        }
    }
}
