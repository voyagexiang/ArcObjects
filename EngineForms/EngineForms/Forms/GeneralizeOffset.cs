using DevExpress.XtraEditors;
using ESRI.ArcGIS.Carto;
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
        public offset()
        {
            InitializeComponent();
        }
        public offset(ILayer mLayer)
        {
            InitializeComponent();
            this.mLayer = mLayer;
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
            try {
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
                this.Dispose();
            } catch (Exception ex) {
                XtraMessageBox.Show("简化失败","提示信息",MessageBoxButtons.OK);
            }
           
          
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
