using ExpressBase.Common.Constants;
using ExpressBase.Common.Data;
using ExpressBase.Common.ServiceClients;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.Messaging;
using ServiceStack.RabbitMq;

namespace ExpressBase.StaticFileServer
{
    public class BaseService : Service
    {
        protected EbConnectionFactory EbConnectionFactory { get; private set; }

        private EbConnectionFactory _infraConnectionFactory = null;

        protected RabbitMqProducer MessageProducer3 { get; private set; }

        protected RabbitMqQueueClient MessageQueueClient { get; private set; }

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

        public BaseService()
        {
        }

        public BaseService(IEbConnectionFactory _dbf)
        {
            Log.Info("In Base Service 1");
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

        public BaseService(IEbConnectionFactory _dbf, IMessageProducer _msp, IMessageQueueClient _mqc, IEbServerEventClient _sec, IEbMqClient _mq)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
            this.MessageProducer3 = _msp as RabbitMqProducer;
            this.MessageQueueClient = _mqc as RabbitMqQueueClient;
            this.ServerEventClient = _sec as EbServerEventClient;
            this.MqClient = _mq as EbMqClient;
        }

        public BaseService(IEbConnectionFactory _dbf, IMessageProducer _msp, IMessageQueueClient _mqc, IEbServerEventClient _sec)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
            this.MessageProducer3 = _msp as RabbitMqProducer;
            this.MessageQueueClient = _mqc as RabbitMqQueueClient;
            this.ServerEventClient = _sec as EbServerEventClient;
        }

        public BaseService(IEbConnectionFactory _dbf, IMessageProducer _msp, IMessageQueueClient _mqc)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
            this.MessageProducer3 = _msp as RabbitMqProducer;
            this.MessageQueueClient = _mqc as RabbitMqQueueClient;
        }

        public ILog Log { get { return LogManager.GetLogger(GetType()); } }
    }
}