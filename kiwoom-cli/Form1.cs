using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kiwoom_cli
{
    public partial class Form1 : Form
    {
        Kiwoom.Api api = new Kiwoom.Api();
        public Form1()
        {
            Visible = false;
            InitializeComponent();

            Console.WriteLine("hello");

            
            api.Connect();

            api.OnConnected += Api_OnConnected;
            api.OnReceiveTrCondition += Api_OnReceiveTrCondition;
        }

        private void Api_OnReceiveTrCondition(Kiwoom.Api sender, string[] strCodeList, Kiwoom.ConditionInfo info)
        {
            foreach (string item in strCodeList)
            {
                Console.WriteLine(item);
            }
        }

        private void Api_OnConnected(Kiwoom.Api sender)
        {
            Console.WriteLine("Connected");

            Kiwoom.ConditionInfo[] list = api.GetConditionInfoList();

            foreach (Kiwoom.ConditionInfo item in list)
            {
                Console.WriteLine(item.Name);
                if (item.Name == "동전주목록")
                    api.RequestSearchCondition(item);
            }

            Console.WriteLine(list.Length.ToString());
            Console.WriteLine(Console.ReadLine());
            //throw new NotImplementedException();
        }

        protected override void SetVisibleCore(bool value)
        {
            value = false;
            base.SetVisibleCore(value);
        }
    }
}
