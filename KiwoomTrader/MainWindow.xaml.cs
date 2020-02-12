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
        private Kiwoom.Api m_ctrl = null;
        private ILog log = null;

        public class Item
        {
            public string code
            {
                get;
                set;
            }

            public Item(string code)
            {
                this.code = code;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Log.Init();

            log = Log.Get(this.GetType());
            
            // 키움API객체 생성
            m_ctrl = new Kiwoom.Api();
            m_ctrl.OnConnected += M_ctrl_OnConnected;
            m_ctrl.OnConnectError += M_ctrl_OnConnectError;
            m_ctrl.OnReceiveTrCondition += M_ctrl_OnReceiveTrCondition;

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
                m_ctrl.RequestSearchCondition(info);
            e.Handled = true;
        }

        private void M_ctrl_OnConnectError(Api sender, Api.ErrorCode code)
        {
            MessageBox.Show(string.Format("접속실패, 오류코드={0}", code));
        }

        private void M_ctrl_OnReceiveTrCondition(Api sender, string[] strCodeList, ConditionInfo info)
        {
            uint idx = 0;

            List<Item> items = new List<Item>();

            foreach(string code in strCodeList)
            {
                items.Add(new Item(code));
                log.Debug("조건검색 결과, 조건식명=" + info.Name + ",[" + idx.ToString() + "], code=" + code);
                idx++;
            }

            m_grid.ItemsSource = items;

            m_ctrl.RequestData(strCodeList);
        }

        private void M_ctrl_OnConnected(Kiwoom.Api sender)
        {
            m_btnDisconnect.IsEnabled = true;
            button.IsEnabled = false;

            m_tbLog.Text = "연결되었습니다!\n";
            m_tbLog.Text += "사용자ID=" + m_ctrl.UserID + "\n";
            m_tbLog.Text += "사용자명=" + m_ctrl.UserName + "\n";
            m_tbLog.Text += "계좌목록\n";

            string[] accList = m_ctrl.Accounts;
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
            Kiwoom.ConditionInfo[] condList = m_ctrl.GetConditionInfoList();

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
                m_ctrl.Disconnect();
                button.IsEnabled = true;
                m_btnDisconnect.IsEnabled = false;
            }   
            else if (sender == button)
            {
                if (!m_ctrl.IsConnected)
                    m_ctrl.Connect();
            }
            else if(sender == m_btnRequestCondition)
            {
                RefreshSearchConditionCombobox(ref m_cbSearchCondition);
            }
        }
           

        private void Window_Closed(object sender, EventArgs e)
        {
            m_ctrl.Disconnect();
            log.Debug("Close...");
            m_ctrl.Dispose();
        }
    }
}
