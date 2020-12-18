using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Kiwoom;
using log4net;

namespace Trader
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private Kiwoom.Api api_ = null;
        private ILog log = null;

        public class Item
        {
            /// <summary>
            /// 종목코드
            /// </summary>
            public string code
            {
                get;
                set;
            }
                
            /// <summary>
            /// 종목명
            /// </summary>
            public string name
            { get; set; }

            public Item(string code, string name)
            {
                this.code = code;
                this.name = name;
            }
        }

        /// <summary>
        /// 키움API객체 생성
        /// </summary>
        private void CreateKiwoomApi()
        {
            api_ = new Kiwoom.Api();
            api_.OnConnected += api__OnConnected;
            api_.OnConnectError += api__OnConnectError;
            api_.OnReceiveTrCondition += api__OnReceiveTrCondition;
            api_.OnReceiveTrData += Api__OnReceiveTrData;
        }

        private void Api__OnReceiveTrData(Api sender, ReceiveTrDataInfo info)
        {
            for (int i = 0; i < info.nDataLength; i++)
            {
                string sCode = api_.GetCommData(info, i, "종목코드");
                string sName = api_.GetCommData(info, i, "종목명");
                string sPrice = api_.GetCommData(info, i, "현재가");


                log.Debug("[" + i.ToString("00") + "] code=" + sCode + ", name=" + sName + ", price=" + sPrice);

                Item item = m_grid.Items.GetItemAt(i) as Item;
                item.name = sName;

            }
            m_grid.Items.Refresh();
        }

        public MainWindow()
        {
            InitializeComponent();
            Log.Init();

            log = Log.Get(this.GetType());

            // 키움API객체 생성
            CreateKiwoomApi();

            // 그리드 생성
            DataGridTextColumn col = new DataGridTextColumn();
            col.Binding = new Binding("code");
            col.Header = "종목코드";
            //m_grid.Columns.Add(col);

            m_cbSearchCondition.SelectionChanged += M_cbSearchCondition_SelectionChanged;
        }

        private void M_cbSearchCondition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;


            m_grid.ItemsSource = null;
                
            ConditionInfo info = (ConditionInfo)cb.SelectedValue;
            if (info != null)
                api_.RequestSearchCondition(info);
        }

        private void api__OnConnectError(Api sender, Api.ErrorCode code)
        {
            MessageBox.Show(string.Format("접속실패, 오류코드={0}", code));
        }

        private void api__OnReceiveTrCondition(Api sender, string[] strCodeList, ConditionInfo info)
        {
            uint idx = 0;

            List<Item> items = new List<Item>();

            foreach(string code in strCodeList)
            {
                items.Add(new Item(code, ""));
                log.Debug("조건검색 결과, 조건식명=" + info.Name + ",[" + idx.ToString() + "], code=" + code);
                idx++;
            }

            m_grid.ItemsSource = items;

            if(api_.RequestData(strCodeList)== Api.ErrorCode.CUSTOM_ERR_REQUEST_LIMIT_EXCEEDED)
            {
                List<String> codelist = new List<String>();
                foreach(string code in strCodeList.Take(100))
                {
                    codelist.Add(code);
                }
                // 100개 초과시 일단 100개로 줄여서 요청
                api_.RequestData(codelist.ToArray());
            }
        }

        private void api__OnConnected(Kiwoom.Api sender)
        {
            m_btnDisconnect.IsEnabled = true;
            button.IsEnabled = false;

            m_tbLog.Text = "연결되었습니다!\n";
            m_tbLog.Text += "사용자ID=" + api_.UserID + "\n";
            m_tbLog.Text += "사용자명=" + api_.UserName + "\n";
            m_tbLog.Text += "계좌목록\n";

            string[] accList = api_.Accounts;
            foreach(string acc in accList)
            {   
                m_tbLog.Text += acc + "\n";
            }
        }

        /// <summary>
        /// 사용자 조건검색식 콤보에 저장하기
        /// </summary>
        private void RefreshSearchConditionCombobox(ref ComboBox cbox)
        {
            // 기존 콤보값 삭제
            if (cbox.HasItems)
            {
                cbox.Items.Clear();
            }
            Kiwoom.ConditionInfo[] condList = api_.GetConditionInfoList();

            cbox.DisplayMemberPath = "Key";
            cbox.SelectedValuePath = "Value";

            foreach (Kiwoom.ConditionInfo info in condList)
            {
                cbox.Items.Add(new KeyValuePair<string, ConditionInfo>(info.Name, info));
            }

            if (cbox.HasItems)
                cbox.SelectedIndex = 0;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == m_btnDisconnect)
            {
                api_.Dispose();
                api_ = null;
                CreateKiwoomApi();
                button.IsEnabled = true;
                m_btnDisconnect.IsEnabled = false;
            }   
            else if (sender == button)
            {
                if (!api_.IsConnected)
                    api_.Connect();
            }
            else if(sender == m_btnRequestCondition)
            {
                if(!api_.IsConnected)
                {
                    MessageBox.Show("접속상태가 아닙니다.");
                    return;
                }
                RefreshSearchConditionCombobox(ref m_cbSearchCondition);
            }
        }
           

        private void Window_Closed(object sender, EventArgs e)
        {            
            log.Debug("Close...");
            api_.Dispose();
        }
    }
}
