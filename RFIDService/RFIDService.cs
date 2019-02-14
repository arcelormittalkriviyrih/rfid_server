using MessageLog;
using RFID;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

namespace RFIDService
{
    public partial class RFIDService : ServiceBase
    {



        private int interval_Y = 60; // секунд
        private int interval_X = 60; // секунд
 
        System.Timers.Timer timer_services_Y = new System.Timers.Timer();

        System.Timers.Timer timer_services_X = new System.Timers.Timer();

        private RFIDThead rfid_thead = new RFIDThead();

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public long dwServiceType;
            public ServiceState dwCurrentState;
            public long dwControlsAccepted;
            public long dwWin32ExitCode;
            public long dwServiceSpecificExitCode;
            public long dwCheckPoint;
            public long dwWaitHint;
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        public RFIDService(string[] args)
        {
            Logger.InitLogger();//инициализация - требуется один раз в начале
            Logger.Log.Info("RFIDService - создан");
            InitializeComponent();
            InitializeService();
        }

        #region Управление службой
        /// <summary>
        /// Инициализация сервиса (проверка данных в БД и создание settings)
        /// </summary>
        public void InitializeService() 
        {
            try
            {
                timer_services_Y.Interval = this.interval_Y * 1000;
                timer_services_Y.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimerServicesY);

                timer_services_X.Interval = this.interval_Y * 1000;
                timer_services_X.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimerServicesX);
                
                //Добавить инициализацию других таймеров
                //...............
            }
            catch (Exception e)
            {
                Logger.Log.Error(e);
                return;
            } 
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            // Запустить таймера потоков
            rfid_thead.StartY();
            rfid_thead.StartX();
            //TODO: Добавить запуск других таймеров
            //...............

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            // Отправить сообщение
            Logger.Log.Info("RFIDService - запущен");
        }

        protected override void OnStop()
        {
            // Добавить остановку других таймеров
            //...............
            // Отправить сообщение
            Logger.Log.Info("RFIDService - Остановлен");

        }
        #endregion

        #region Y
        private void OnTimerServicesY(object sender, System.Timers.ElapsedEventArgs args)
        {
            try
            {
                rfid_thead.StartY();
            }
            catch (Exception e)
            {
                Logger.Log.Error(e);
            }
        }
        #endregion

        #region X
        private void OnTimerServicesX(object sender, System.Timers.ElapsedEventArgs args)
        {
            try
            {
                rfid_thead.StartX();
            }
            catch (Exception e)
            {
                Logger.Log.Error(e);
            }
        }
        #endregion

    }
}
