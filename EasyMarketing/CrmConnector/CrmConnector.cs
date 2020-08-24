using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.CrmConnector
{
    public class CrmConnector : IDisposable
    {
        private IOrganizationService _service;
        private CrmServiceClient _crmCon;
        //private OrganizationServiceContextMultipleRequest _serviceContextMultipleRequest;


        public IOrganizationService service
        {
            get
            {
                return _service;
            }
        }

        public CrmServiceClient crmCon
        {
            get
            {
                return _crmCon;
            }
        }

        //public OrganizationServiceContextMultipleRequest serviceContextMultipleRequest
        //{
        //    get
        //    {
        //        return _serviceContextMultipleRequest;
        //    }
        //}


        /// <summary>
        /// Default constructor
        /// </summary>
        public CrmConnector()
        {
            //Timeout = 10min
            Nullable<TimeSpan> timeOut = TimeSpan.FromMinutes(10);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["CrmConn"].ToString();
                if (connectionString == null)
                {
                    throw new Exception("A connectionString para ligação ao CRM não está definida");
                }

                _crmCon = new CrmServiceClient(connectionString);

                //var proxy = _crmCon.OrganizationServiceProxy;
                //if (proxy != null)
                //{
                //    proxy.ServiceConfiguration.CurrentServiceEndpoint.Binding.CloseTimeout = timeOut.Value;
                //    proxy.ServiceConfiguration.CurrentServiceEndpoint.Binding.OpenTimeout = timeOut.Value;
                //    proxy.ServiceConfiguration.CurrentServiceEndpoint.Binding.ReceiveTimeout = timeOut.Value;
                //    proxy.ServiceConfiguration.CurrentServiceEndpoint.Binding.SendTimeout = timeOut.Value;
                //    proxy.Timeout = timeOut.Value;
                //}


                _service = (IOrganizationService)_crmCon.OrganizationWebProxyClient != null ? (IOrganizationService)_crmCon.OrganizationWebProxyClient : (IOrganizationService)_crmCon.OrganizationServiceProxy;
                //_serviceContextMultipleRequest = new OrganizationServiceContextMultipleRequest(_service);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("CrmConnector: Ocorreu um Erro na Conexção ao CRM. Mensagem: {0}", ex.Message));
            }

        }

        public void Dispose()
        {

        }
    }
}

