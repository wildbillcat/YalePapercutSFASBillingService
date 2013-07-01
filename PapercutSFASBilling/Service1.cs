using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PapercutSFASBilling
{
    public partial class Service1 : ServiceBase
    {

        BillingManager billmgr;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            billmgr = new BillingManager();
            GC.KeepAlive(billmgr);
        }

        protected override void OnStop()
        {
            billmgr.EndBilling(); 
        }
      
    }
}
