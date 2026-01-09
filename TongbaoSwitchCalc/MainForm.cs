using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TongbaoSwitchCalc.DataModel;
using TongbaoSwitchCalc.Impl;

namespace TongbaoSwitchCalc
{
    public partial class MainForm : Form
    {
        private PlayerData mPlayerData;
        private IRandomGenerator mRandom;

        private SquadType mSelectedSquadType = SquadType.Flower;
        private int mSelectedTongbaoPosIndex = -1;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= 0x02000000; //双缓冲
                return createParams;
            }
        }

        public MainForm()
        {
            InitializeComponent();
            InitDataModel();
            InitView();

            //DataModelTest();
        }

        private void InitDataModel()
        {
            Helper.InitConfig();
            mRandom = new RandomGenerator();
            mPlayerData = new PlayerData(mRandom);
            InitPlayerData();
        }

        private void InitPlayerData()
        {
            mPlayerData.Init(mSelectedSquadType, new Dictionary<ResType, int>()
            {
                { ResType.LifePoint, 1 },
                { ResType.OriginiumIngots, 0 },
                { ResType.Coupon, 0 },
                { ResType.Candles, 0 },
                { ResType.Shield, 0 },
                { ResType.Hope, 0 },
            });
        }

        private void DataModelTest()
        {
            string[] names = new string[] { "大炎通宝", "奇土生金", "水生木护", "金寒水衍", "投木炎延",
            "西廉贞", "北刺面", "南见山", "东缺角", };

            foreach (var name in names)
            {
                TongbaoConfig config = Helper.GetTongbaoConfigByName(name);
                if (config != null)
                {
                    Tongbao tongbao = Tongbao.CreateTongbao(config.Id);
                    mPlayerData.AddTongbao(tongbao);
                }
            }

            using (CodeTimer ct = new CodeTimer("Test"))
            {
                for (int i = 0; i < 1000; i++)
                {
                    SelectTongbaoPos(i % names.Length);
                    SwitchOnce(true);
                }
            }

            for (int i = 0; i < mPlayerData.TongbaoBox.Length; i++)
            {
                string tongbaoName = mPlayerData.TongbaoBox[i] != null ?
                    Helper.GetTongbaoName(mPlayerData.TongbaoBox[i].Id) : "Empty";
                Helper.Log($"[{i}]={tongbaoName}");
            }

            foreach (ResType type in Enum.GetValues(typeof(ResType)))
            {
                Helper.Log($"[{Helper.GetResName(type)}]={mPlayerData.GetResValue(type)}");
            }
        }

        private void InitView()
        {
            Helper.InitResources();

            comboBoxSquad.DisplayMember = "Key";
            comboBoxSquad.ValueMember = "Value";

            comboBoxSquad.Items.Clear();
            foreach (SquadType type in Enum.GetValues(typeof(SquadType)))
            {
                comboBoxSquad.Items.Add(new SquadComboBoxItem(type));
            }

            comboBoxSquad.SelectedIndex = 0;

            checkBoxFortune.Checked = false;

            InitTongbaoView();
        }

        private void InitTongbaoView()
        {
            // InitImageList
            listViewTongbao.LargeImageList?.Dispose();
            ImageList imageList = new ImageList
            {
                ImageSize = new Size(48, 48),
            };

            if (Helper.TongbaoSlotImage != null)
            {
                imageList.Images.Add("Empty", Helper.TongbaoSlotImage);
            }
            foreach (var item in TongbaoConfig.GetAllTongbaoConfigs())
            {
                TongbaoConfig config = item.Value;
                Image image = Helper.GetTongbaoImage(config.Id);
                if (image != null)
                {
                    imageList.Images.Add(config.Id.ToString(), image);
                }
            }
            listViewTongbao.LargeImageList = imageList;

            // InitSlot
            InitTongbaoSlot();
            listViewTongbao.SelectedItems.Clear();
        }

        private void InitTongbaoSlot()
        {
            listViewTongbao.Items.Clear();
            for (int i = 0; i < mPlayerData.MaxTongbaoCount; i++)
            {
                ListViewItem item = new ListViewItem
                {
                    Name = "TongbaoSlot_" + (i + 1),
                    Text = $"[{i + 1}] (空)",
                    ToolTipText = "双击添加/修改通宝",
                    ImageKey = "Empty",
                };
                listViewTongbao.Items.Add(item);
                UpdateTongbaoView(i);
            }
        }

        private void UpdateTongbaoView(int posIndex)
        {
            if (posIndex < 0 || posIndex >= listViewTongbao.Items.Count)
            {
                return;
            }

            Tongbao tongbao = mPlayerData.GetTongbao(posIndex);
            ListViewItem item = listViewTongbao.Items[posIndex];
            if (tongbao != null)
            {
                string name = Helper.GetTongbaoName(tongbao.Id);
                if (tongbao.RandomResType != ResType.None)
                {
                    name += $"(品相：{Helper.GetResName(tongbao.RandomResType)}+{tongbao.RandomResCount})";
                }
                item.Text = $"[{posIndex + 1}] {name}";
                item.ImageKey = tongbao.Id.ToString();
            }
            else
            {
                item.Text = $"[{posIndex + 1}] (空)";
                item.ImageKey = "Empty";
            }
        }

        private void UpdateView()
        {

        }

        private void OnSelectNewRandomTongbao(int id, int posIndex)
        {
            Tongbao tongbao = Tongbao.CreateTongbao(id, mRandom);
            mPlayerData.InsertTongbao(tongbao, posIndex);
            UpdateTongbaoView(posIndex);
        }

        private void OnSelectNewCustomTongbao(int id, int posIndex,
            ResType randomResType = ResType.None, int randomResCount = 0)
        {
            Tongbao tongbao = Tongbao.CreateTongbao(id);
            tongbao.ApplyRandomRes(randomResType, randomResCount);
            mPlayerData.InsertTongbao(tongbao, posIndex);
            UpdateTongbaoView(posIndex);
        }

        private void SelectTongbaoPos(int posIndex)
        {
            mSelectedTongbaoPosIndex = posIndex;
            UpdateView();
        }

        private void StartSwitchSimulation()
        {

        }

        private void SwitchOnce(bool force = false)
        {
            int posIndex = mSelectedTongbaoPosIndex;
            if (!mPlayerData.SwitchTongbao(posIndex, force))
            {
                Tongbao tongbao = mPlayerData.GetTongbao(posIndex);
                if (tongbao == null)
                {
                    MessageBox.Show("交换失败，请先选中一个通宝。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!tongbao.CanSwitch())
                {
                    MessageBox.Show($"交换失败，选中通宝[{Helper.GetTongbaoName(tongbao.Id)}]无法交换。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!force && !mPlayerData.HasEnoughSwitchLife())
                {
                    MessageBox.Show($"交换失败，当前生命值不足", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                MessageBox.Show("交换失败，请检查当前配置和状态。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            UpdateView();
        }

        private void cbSquad_SelectedIndexChanged(object sender, EventArgs e)
        {
            SquadComboBoxItem item = comboBoxSquad.SelectedItem as SquadComboBoxItem;
            if (mPlayerData.SwitchCount > 0)
            {
                var result = MessageBox.Show("切换分队会重置当前交换次数，是否继续？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    return;
                }
            }

            mSelectedSquadType = item?.Value ?? SquadType.Flower;
            mPlayerData.SetSquadType(mSelectedSquadType);
            InitTongbaoSlot();
        }

        private void checkBoxFortune_CheckedChanged(object sender, EventArgs e)
        {
            mPlayerData.SetSpecialCondition(SpecialConditionFlag.Collectible_Fortune, checkBoxFortune.Checked);
        }

        // 选择通宝槽位
        private void listViewTongbao_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewTongbao.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewTongbao.SelectedItems[0];
                int posIndex = listViewTongbao.Items.IndexOf(selectedItem);
                SelectTongbaoPos(posIndex);
            }
        }

        // 双击通宝槽位
        private void listViewTongbao_ItemActivate(object sender, EventArgs e)
        {
            if (listViewTongbao.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewTongbao.SelectedItems[0];
                int posIndex = listViewTongbao.Items.IndexOf(selectedItem);

                // 测试，随机添加通宝
                var configs = TongbaoConfig.GetAllTongbaoConfigs();
                int random = mRandom.Next(0, configs.Count);
                int index = 0;
                int targetId = -1;
                foreach (var item in configs)
                {
                    if (index == random)
                    {
                        targetId = item.Value.Id;
                        break;
                    }
                    index++;
                }
                if (targetId > 0)
                {
                    OnSelectNewRandomTongbao(targetId, posIndex);
                }
            }
        }
    }
}
