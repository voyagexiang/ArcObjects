using DevExpress.XtraEditors;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
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

namespace EngineForms
{
    public partial class offset : DevExpress.XtraEditors.XtraForm
    {
        ILayer mLayer;
        AxMapControl axMapControl;
        IMap mMap;
        public offset()
        {
            InitializeComponent();
        }
        public offset(ILayer mLayer, AxMapControl axMapControl)
        {
            InitializeComponent();
            this.mLayer = mLayer;
            this.axMapControl = axMapControl;
            mMap = axMapControl.Map;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            double offsetValue = -1;
            try
            {
                offsetValue = Convert.ToDouble(textEdit1.Text);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("请输入数值类型", "提示信息", MessageBoxButtons.OK);
                return;
            }
            try
            {
                //启动编辑
                IFeatureLayer featureLayer = mLayer as IFeatureLayer;
                IFeatureClass pFeatureClass = featureLayer.FeatureClass;

                IWorkspace workspace=null;
                IEngineEditor mEngineEditor = mEngineEditor = new EngineEditorClass();
                if (pFeatureClass.FeatureDataset != null)
                {
                    workspace = pFeatureClass.FeatureDataset.Workspace;
                    mEngineEditor.EditSessionMode = esriEngineEditSessionMode.esriEngineEditSessionModeVersioned;
                    mEngineEditor.StartEditing(workspace, mMap);
                    ((IEngineEditLayers)mEngineEditor).SetTargetLayer(featureLayer, -1);
                    mEngineEditor.StartOperation();
                }



                ISelectionSet mSelectionSet = (mLayer as IFeatureSelection).SelectionSet;
                ICursor mCursor;
                mSelectionSet.Search(null, false, out mCursor);

                IFeature mFeature = mCursor.NextRow() as IFeature;
                while (mFeature != null)
                {
                    IGeometry geometry = mFeature.ShapeCopy;
                    IPolycurve polycurve = geometry as IPolycurve;
                    polycurve.Generalize(offsetValue);
                    mFeature.Shape = polycurve as IGeometry;
                    mFeature.Store();
                    mFeature = mCursor.NextRow() as IFeature;
                }
                if (workspace != null)
                {
                    mEngineEditor.StopEditing(true);
                }

                this.Dispose();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("简化失败", "提示信息", MessageBoxButtons.OK);
            }


        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
