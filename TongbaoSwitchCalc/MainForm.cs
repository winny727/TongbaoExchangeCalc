using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TongbaoSwitchCalc.DataModel;
using TongbaoSwitchCalc.Impl;

namespace TongbaoSwitchCalc
{
    public partial class MainForm : Form
    {
        private PlayerData mPlayerData;

        public MainForm()
        {
            InitializeComponent();
            InitDataModel();
        }

        private void InitDataModel()
        {
            Helper.InitConfig();
            mPlayerData = new PlayerData(new RandomGenerator());
        }
    }
}
