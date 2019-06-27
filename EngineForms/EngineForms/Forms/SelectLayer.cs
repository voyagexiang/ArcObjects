using DevExpress.XtraEditors;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EngineForms
{
    public partial class SelectLayer : DevExpress.XtraEditors.XtraForm
    {
        AxMapControl mMap;
        IEngineEditor mEngineEditor;
        private esriGeometryType mLayerType;
        public SelectLayer(AxMapControl mMap, IEngineEditor mEngineEditor, esriGeometryType mLayerType)
        {
            InitializeComponent();
            this.mMap = mMap;
            this.mEngineEditor = mEngineEditor;
            this.mLayerType = mLayerType;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string layerName = comboBox1.Text;
            this.Close();

            int count = mMap.LayerCount;
            for (int i = 0; i < count; i++)
            {
                if (((IFeatureLayer)mMap.get_Layer(i)).FeatureClass.ShapeType == mLayerType && mMap.get_Layer(i).Name == layerName)
                {
                    ILayer currentLayer = mMap.get_Layer(i);
                    IFeatureLayer featureLayer = currentLayer as IFeatureLayer;

                    ((IEngineEditLayers)mEngineEditor).SetTargetLayer(featureLayer, 0);
                    ICommand pCmd = new ControlsEditingPasteCommandClass();
                    pCmd.OnCreate(mMap.Object);
                    mMap.CurrentTool = pCmd as ITool;
                    pCmd.OnClick();
                    XtraMessageBox.Show("粘贴成功", "提示信息", MessageBoxButtons.OK);
                    return;
                }

            }
        }

        private void SelectLayer_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            int count = mMap.LayerCount;
            for (int i = 0; i < count; i++)
            {
                if (((IFeatureLayer)mMap.get_Layer(i)).FeatureClass.ShapeType == mLayerType)
                {
                    string mLayerName = mMap.get_Layer(i).Name;
                    comboBox1.Items.Add(mLayerName);
                }

            }
            if (comboBox1.Items.Count != 0)
            {
                comboBox1.Text = comboBox1.Items[0].ToString();
            }
        }
    }
}
