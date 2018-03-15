using ExpressBase.Common.Constants;
using ExpressBase.Common.Data;
using ExpressBase.Common.ServiceClients;
using ServiceStack;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressBase.StaticFileServer
{
    public class BaseService : Service
    {
        protected EbConnectionFactory EbConnectionFactory { get; private set; }

        private EbConnectionFactory _infraConnectionFactory = null;

        protected EbServerEventClient ServerEventClient { get; private set; }

        protected EbMqClient MqClient { get; private set; }

        protected EbConnectionFactory InfraConnectionFactory
        {
            get
            {
                if (_infraConnectionFactory == null)
                    _infraConnectionFactory = new EbConnectionFactory(CoreConstants.EXPRESSBASE, this.Redis);

                return _infraConnectionFactory;
            }
        }

        public BaseService() { }

        public BaseService(IEbConnectionFactory _dbf)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
        }

        public BaseService(IEbConnectionFactory _dbf, IEbServerEventClient _sec)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
            this.ServerEventClient = _sec as EbServerEventClient;
        }

        public BaseService(IEbConnectionFactory _dbf, IEbMqClient _mqc)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
            this.MqClient = _mqc as EbMqClient;
        }

        public BaseService(IEbConnectionFactory _dbf, IEbServerEventClient _sec, IEbMqClient _mqc)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
            this.ServerEventClient = _sec as EbServerEventClient;
            this.MqClient = _mqc as EbMqClient;
        }

        public ILog Log { get { return LogManager.GetLogger(GetType()); } }
    }
}
