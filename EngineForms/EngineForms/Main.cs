using DevExpress.XtraEditors;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Display;
using EngineForms.Command;
using EngineForms.Forms;

namespace EngineForms
{
    public partial class Main : DevExpress.XtraEditors.XtraForm
    {

        public ILayer layer;
        private IEngineEditor mEngineEditor;
        private esriGeometryType mLayerType;
        private ILayer currentLayer;
        private bool splitFeature;
        private IMap mMap;

        public Main()
        {
            InitializeComponent();
            mEngineEditor = new EngineEditorClass();
            mMap = this.axMapControl1.Map;
        }

        private void axTOCControl1_OnMouseDown(object sender, AxESRI.ArcGIS.Controls.ITOCControlEvents_OnMouseDownEvent e)
        {

        }

        private void axMapControl1_OnMouseDown(object sender, AxESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e)
        {

        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = OpenMxd();
            if (path != "")
                axMapControl1.LoadMxFile(path);
            //打开shapefile文件
            //string[] ShpFile =OpenShapeFile();
            //axMapControl1.AddShapeFile(ShpFile[0],ShpFile[1]);
        }

        public string OpenMxd()
        {
            string MxdPath = "";
            OpenFileDialog OpenMXD = new OpenFileDialog();
            OpenMXD.Title = "打开地图";
            OpenMXD.InitialDirectory = "H:";
            if (OpenMXD.ShowDialog() == DialogResult.OK)
            {
                MxdPath = OpenMXD.FileName;
            }
            return MxdPath;
        }
        public string[] OpenShapeFile()
        {
            string[] ShpFile = new string[2];
            OpenFileDialog OpenShpFile = new OpenFileDialog();
            OpenShpFile.Title = "打开地图";
            OpenShpFile.InitialDirectory = "H:";
            OpenShpFile.Filter = "Shap文件(*.shp)|*.shp";
            if (OpenShpFile.ShowDialog() == DialogResult.OK)
            {
                string ShapPath = OpenShpFile.FileName;

                int Position = ShapPath.LastIndexOf("\\");

                string FilePath = ShapPath.Substring(0, Position);

                string ShpName = ShapPath.Substring(Position + 1);
                ShpFile[0] = FilePath;
                ShpFile[1] = ShpName;
            }
            return ShpFile;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string PDBPath = mdbPath();

            if (PDBPath != "")
            {
                IWorkspaceFactory pWSFactory = new AccessWorkspaceFactoryClass();

                IWorkspace pWS = pWSFactory.OpenFromFile(PDBPath, 0);

                IEnumDataset pEDataset = pWS.get_Datasets(esriDatasetType.esriDTAny);

                IDataset pDataset = pEDataset.Next();
                comboBox1.Items.Clear();

                while (pDataset != null)
                {
                    if (pDataset.Type == esriDatasetType.esriDTFeatureClass)
                    {
                        comboBox1.Items.Add(pDataset.Name);
                    }
                    pDataset = pEDataset.Next();
                }
                comboBox1.Text = comboBox1.Items[0].ToString();

            }
        }
        public string mdbPath()
        {
            string path = "";
            OpenFileDialog OpenFile = new OpenFileDialog();
            OpenFile.Filter = "个人数据库(*.mdb)|*.mdb";
            DialogResult DialogR = OpenFile.ShowDialog();
            if (DialogR == DialogResult.Cancel) { }
            else
            {
                path = OpenFile.FileName;
            }

            return path;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            axMapControl1.Map.ClearSelection();
            axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            axMapControl1.Map.ClearLayers();
            string _pInstance = "sde:oracle11g:172.31.2.90/orcl";
            string _pUser = "cstest";
            string _pPassword = "cstest";
            string _pVersion = "sde.DEFAULT";
            IWorkspace pWS = null;

            pWS = GetSDEWorkspace(_pInstance, _pUser, _pPassword, _pVersion) as IWorkspace;

            try
            {
                IEnumDatasetName pEDatasetNames = pWS.get_DatasetNames(esriDatasetType.esriDTAny);



                IDatasetName pDatasetName = pEDatasetNames.Next();
                while (pDatasetName != null)
                {

                    string name = pDatasetName.Name;
                    if (pDatasetName.Name.Equals("CSTEST.QDTEST"))
                    {
                        if (pDatasetName.Type.Equals(esriDatasetType.esriDTFeatureDataset))
                        {
                            IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWS;
                            IFeatureDataset pFeatureDataset = pFeatureWorkspace.OpenFeatureDataset("CSTEST.QDTEST");
                            IEnumDataset pEnumDataset = pFeatureDataset.Subsets;
                            pEnumDataset.Reset();
                            IDataset pDataset = pEnumDataset.Next();
                            comboBox1.Items.Clear();

                            while (pDataset != null)
                            {
                                if (pDataset.Type == esriDatasetType.esriDTFeatureClass)
                                {
                                    comboBox1.Items.Add(pDataset.Name);
                                    if (pDataset is IFeatureClass)
                                    {
                                        IFeatureLayer pFeatureLayer = new FeatureLayerClass();
                                        pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(pDataset.Name);
                                        pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;

                                        axMapControl1.Map.AddLayer(pFeatureLayer);
                                    }
                                }
                                pDataset = pEnumDataset.Next();
                            }
                            comboBox1.Text = comboBox1.Items[0].ToString();
                        }
                    }

                    pDatasetName = pEDatasetNames.Next();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }

        public IWorkspace GetSDEWorkspace(string _pInstance, string _pUser, string _pPassword, string _pVersion)
        {
            IPropertySet pPropertySet = new PropertySetClass();
            pPropertySet.SetProperty("INSTANCE", _pInstance);
            pPropertySet.SetProperty("USER", _pUser);
            pPropertySet.SetProperty("PASSWORD", _pPassword);
            pPropertySet.SetProperty("VERSION", _pVersion);

            IWorkspace pWks = null;

            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)new SdeWorkspaceFactoryClass();

            try
            {
                pWks = workspaceFactory.Open(pPropertySet, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }

            return pWks;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (mEngineEditor.EditState == esriEngineEditState.esriEngineStateNotEditing)
            {
                AxMapControl mMap =this.axMapControl1;
                currentLayer = GetTocSelectedLayer();
                if (currentLayer is IFeatureLayer)
                {
                    IFeatureLayer featureLayer = currentLayer as IFeatureLayer;
                    mLayerType = featureLayer.FeatureClass.ShapeType;

                    IDataset dataset = featureLayer.FeatureClass as IDataset;



                    IVersionedObject verObj = dataset as IVersionedObject;
                    if (verObj != null)
                    {
                        try
                        {
                            verObj.RegisterAsVersioned(true);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                    }
                    //if (verObj != null)
                    //{

                    //    MessageBox.Show("数据集未注册为版本！");
                    //    return;
                    //}
                    IWorkspace workspace;
                    try
                    {
                        mEngineEditor.EditSessionMode = esriEngineEditSessionMode.esriEngineEditSessionModeVersioned;


                        workspace = dataset.Workspace;
                        mEngineEditor.StartEditing(workspace, mMap.Map);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            mEngineEditor.EditSessionMode = esriEngineEditSessionMode.esriEngineEditSessionModeNonVersioned;


                            workspace = dataset.Workspace;
                            mEngineEditor.StartEditing(workspace, mMap.Map);
                        }
                        catch (Exception ex1)
                        {
                            XtraMessageBox.Show(ex1.Message, "提示信息", MessageBoxButtons.OK);
                        }

                    }

                    ((IEngineEditLayers)mEngineEditor).SetTargetLayer(featureLayer, 0);

                }
            }
        }

        /// <summary>
        /// 获取图层控制控件中选中的图层
        /// </summary>
        /// <returns></returns>
        private ILayer GetTocSelectedLayer()
        {
            IBasicMap map = null;
            ILayer layer = null;
            object unk = null;
            object data = null;
            esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
            //esriTOCControlItemNone | esriTOCControlItemMap | esriTOCControlItemLayer | esriTOCControlItemHeading |esriTOCControlItemLegendClass
            axTOCControl1.GetSelectedItem(ref item, ref map, ref layer, ref unk, ref data);
            if (item == esriTOCControlItem.esriTOCControlItemLayer)
            {
                return layer;
            }
            else
            {
                return null;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (mEngineEditor.HasEdits())
            {
                mEngineEditor.StopEditing(true);
            }
            else
            {
                mEngineEditor.StopEditing(false);
            }
            this.axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;

            this.axMapControl1.Refresh();
        }

        private void 打开属性表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AttributeTable attributeTable = new AttributeTable(layer, axMapControl1, axMapControl2);
            attributeTable.Text = "属性表：" + layer.Name;
            attributeTable.ShowDialog();
        }

        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap map = new MapClass();
            layer = new FeatureLayerClass();
            object other = new object();
            object index = new object();

            //Determine what kind of item is selected
            axTOCControl1.HitTest(e.x, e.y, ref item, ref map, ref layer, ref other, ref index);

            if (e.button != 2) return;

            if (item == esriTOCControlItem.esriTOCControlItemLayer)
            {
                contextMenuStrip1.Show(axTOCControl1, new System.Drawing.Point(e.x, e.y));
            }
        }


        private void button8_Click(object sender, EventArgs e)
        {
            bool b = isSelect();
            if (b)
            {
                ICommand pCmd = new ControlsEditingCopyCommandClass();
                pCmd.OnCreate(axMapControl1.Object);
                axMapControl1.CurrentTool = pCmd as ITool;
                pCmd.OnClick();
            }
            else
            {
                XtraMessageBox.Show("请选择要素", "提示信息", MessageBoxButtons.OK);
                return;
            }
        }
        /// <summary>
        /// 判断有没有选择要素
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        bool isSelect()
        {
            bool b = true;

            IMap map = axMapControl1.Map;
            ISelection selection = map.FeatureSelection;
            IEnumFeatureSetup iEnumFeatureSetup = (IEnumFeatureSetup)selection;
            iEnumFeatureSetup.AllFields = true;
            IEnumFeature pEnumFeature = (IEnumFeature)iEnumFeatureSetup;
            pEnumFeature.Reset();

            IFeature pFeature = pEnumFeature.Next();
            if (pFeature == null)
            {
                b = false;
            }
            return b;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (mEngineEditor.EditState == esriEngineEditState.esriEngineStateNotEditing)
            {
                XtraMessageBox.Show("请打开编辑", "提示信息", MessageBoxButtons.OK);
            }
            else
            {
                SelectLayer selectLayer = new SelectLayer(axMapControl1, mEngineEditor, mLayerType);
                selectLayer.Show(axMapControl1);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {

       


            bool b = isSelect();
            if (b)
            {

                //执行命令
                ICommand mCmd = new MergeCommand();

                mCmd.OnCreate(axMapControl1.Object);

                (mCmd as MergeCommand).mLayer = layer;
                (mCmd as MergeCommand).pMapControl = axMapControl1;

                axMapControl1.CurrentTool = mCmd as ITool;
                
            }
            else
            {
                XtraMessageBox.Show("请选择要素", "提示信息", MessageBoxButtons.OK);
                return;
            }
        }
     

        private void button12_Click(object sender, EventArgs e)
        {
            ILayer tocSelectedLayer = GetTocSelectedLayer();

            if (tocSelectedLayer != null)
            {
                IDataset dataset = (tocSelectedLayer as IFeatureLayer).FeatureClass.FeatureDataset as IDataset;
                IVersionedObject verObj = dataset as IVersionedObject;
                if (verObj == null || !verObj.IsRegisteredAsVersioned)
                {
                    try
                    {
                        verObj.RegisterAsVersioned(true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                }
            }
            else
            {
                MessageBox.Show("未选择图层");
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            ICommand mCmd = new EditSplitCommand();
            mCmd.OnCreate(axMapControl1.Object);

            (mCmd as EditSplitCommand).mEngineEditor = mEngineEditor;
            (mCmd as EditSplitCommand).mLayer = layer;
            (mCmd as EditSplitCommand).pMapControl = axMapControl1;
            (mCmd as EditSplitCommand).mMap = axMapControl1.Map;



            axMapControl1.CurrentTool = mCmd as ITool;
            mCmd.OnClick();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            //判断选择的要素图层
            if (layer == null)
            {
                XtraMessageBox.Show("请选择图层", "提示信息", MessageBoxButtons.OK);
                return;
            }
            IFeatureLayer mFeatureLayer = layer as IFeatureLayer;
            IFeatureSelection pFeatureSelection = mFeatureLayer as IFeatureSelection;
            ISelectionSet pSelectionSet = pFeatureSelection.SelectionSet;
            if (pSelectionSet.Count == 0)
            {
                XtraMessageBox.Show("请选择要素", "提示信息", MessageBoxButtons.OK);
                return;
            }
            //执行命令
            ICommand mCmd = new GeneralizeCommand();

            mCmd.OnCreate(axMapControl1.Object);

            (mCmd as GeneralizeCommand).mLayer = layer;
            (mCmd as GeneralizeCommand).pMapControl = axMapControl1;

            axMapControl1.CurrentTool = mCmd as ITool;
           // mCmd.OnClick();
            
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            //判断选择的要素图层
            if (layer == null)
            {
                XtraMessageBox.Show("请选择图层", "提示信息", MessageBoxButtons.OK);
                return;
            }
            IFeatureLayer mFeatureLayer = layer as IFeatureLayer;
            IFeatureSelection pFeatureSelection = mFeatureLayer as IFeatureSelection;
            ISelectionSet pSelectionSet = pFeatureSelection.SelectionSet;
            if (pSelectionSet.Count == 0)
            {
                XtraMessageBox.Show("请选择要素", "提示信息", MessageBoxButtons.OK);
                return;
            }
            //执行命令
            ICommand mCmd = new SmoothCommand();

            mCmd.OnCreate(axMapControl1.Object);

            (mCmd as SmoothCommand).mLayer = layer;
            (mCmd as SmoothCommand).pMapControl = axMapControl1;

            axMapControl1.CurrentTool = mCmd as ITool;
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            XtraForm1 mXtraForm1 = new XtraForm1();
            mXtraForm1.Show(axMapControl1);
            
        }
    }
}
