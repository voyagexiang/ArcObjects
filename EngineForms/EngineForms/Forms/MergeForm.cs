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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EngineForms
{
    public partial class MergeForm : DevExpress.XtraEditors.XtraForm
    {
        private List<string> pItems = new List<string>();
        ILayer mLayer;
        AxMapControl mAxMapControl1;
        IEnumFeature pEnumFeature;

        public MergeForm(ILayer mLayer, AxMapControl mAxMapControl1)
        {
            InitializeComponent();
            this.mLayer = mLayer;
            this.mAxMapControl1 = mAxMapControl1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string saveString = comboBox1.Text;
                // 属性过滤
                IQueryFilter pQueryFilter = new QueryFilter();
                if (pItems.Count > 0)
                {
                    pQueryFilter.WhereClause = saveString;
                }
                else
                {
                    pQueryFilter.WhereClause = null;
                }



                // 要素游标

                //启动编辑
                IFeatureLayer pFeatureLayer = mLayer as IFeatureLayer;
                IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;

                IWorkspace workspace = null;
                IEngineEditor mEngineEditor = mEngineEditor = new EngineEditorClass();
                if (pFeatureClass.FeatureDataset != null)
                {

                    workspace = pFeatureClass.FeatureDataset.Workspace;
                    IMap mMap = mAxMapControl1.Map;
                    mEngineEditor.EditSessionMode = esriEngineEditSessionMode.esriEngineEditSessionModeVersioned;
                    mEngineEditor.StartEditing(workspace, mMap);
                    ((IEngineEditLayers)mEngineEditor).SetTargetLayer(pFeatureLayer, -1);
                    mEngineEditor.StartOperation();

                }



                IFeatureCursor pFCursor = pFeatureClass.Search(pQueryFilter, false);
                IFeature pFeature = pFCursor.NextFeature();
                //todo
                string str = getQueryString();
                pFeature.Shape = GetMergeGeometry(str);

                pFeature.Store();
                //pFCursor.Flush();

                //删除语句
                pItems.Remove(saveString);
                for (int i = 0; i < pItems.Count; i++)
                {
                    delectSelectedCells(pItems[i]);
                }


                this.Close();
                if (workspace != null)
                {
                    mEngineEditor.StopEditing(true);
                }

                XtraMessageBox.Show("合并成功", "提示信息", MessageBoxButtons.OK);
                mAxMapControl1.Refresh();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "合并失败", MessageBoxButtons.OK);
            }

        }


        private void delectSelectedCells(string queryString)
        {
            IFeatureLayer pFeatureLayer = mLayer as IFeatureLayer;
            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;


            IQueryFilter pQueryFilter = new QueryFilterClass();
            pQueryFilter.WhereClause = queryString;
            ITable pTable = pFeatureClass as ITable;
            pTable.DeleteSearchedRows(pQueryFilter);
        }

        private string getQueryString()
        {
            string str = "";
            for (int i = 0; i < pItems.Count; i++)
            {
                if (i == pItems.Count - 1)
                {
                    str += pItems[i];
                }
                else
                {
                    str += pItems[i] + " or ";
                }
            }
            return str;
        }

        /// <summary>
        /// 合并几何体
        /// </summary>
        /// <returns></returns>
        private IGeometry GetMergeGeometry(string str)
        {
            IGeometryBag pGeometryBag = new GeometryBag() as IGeometryBag;
            pGeometryBag.SpatialReference = GetSpatialReference();
            IGeometryCollection pGeometryCollection = pGeometryBag as IGeometryCollection;

            // 属性过滤
            IQueryFilter pQueryFilter = new QueryFilter();
            pQueryFilter.WhereClause = str;

            IFeatureLayer pFeatureLayer = mLayer as IFeatureLayer;
            IFeatureClass in_FeatureClass = pFeatureLayer.FeatureClass;

            // 要素游标
            IFeatureCursor pFeatureCursor = in_FeatureClass.Search(pQueryFilter, true);
            IFeature pFeature = pFeatureCursor.NextFeature();
            if (pFeature == null)
            {
                return null;
            }

            // 遍历游标
            object missing = Type.Missing;
            while (pFeature != null)
            {
                pGeometryCollection.AddGeometry(pFeature.ShapeCopy, ref missing, ref missing);
                pFeature = pFeatureCursor.NextFeature();
            }
            Marshal.ReleaseComObject(pFeatureCursor);

            // 合并要素
            ITopologicalOperator pTopologicalOperator = null;
            if (in_FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
            {
                pTopologicalOperator = new Multipoint() as ITopologicalOperator;
                pTopologicalOperator.ConstructUnion(pGeometryCollection as IEnumGeometry);
            }
            else if (in_FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
            {
                pTopologicalOperator = new Polyline() as ITopologicalOperator;
                pTopologicalOperator.ConstructUnion(pGeometryCollection as IEnumGeometry);
            }
            else
            {
                pTopologicalOperator = new Polygon() as ITopologicalOperator;
                pTopologicalOperator.ConstructUnion(pGeometryCollection as IEnumGeometry);
            }
            return pTopologicalOperator as IGeometry;
        }

        /// <summary>
        /// 获取空间参考
        /// </summary>
        /// <returns></returns>
        private ISpatialReference GetSpatialReference()
        {
            IFeatureLayer pFeatureLayer = mLayer as IFeatureLayer;
            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;

            IGeoDataset pGeoDataset = pFeatureClass as IGeoDataset;
            ISpatialReference pSpatialReference = pGeoDataset.SpatialReference;
            return pSpatialReference;
        }

        private void MargeForm_Load(object sender, EventArgs e)
        {
            IMap map = mAxMapControl1.Map;
            ISelection selection = map.FeatureSelection;
            IEnumFeatureSetup iEnumFeatureSetup = (IEnumFeatureSetup)selection;
            iEnumFeatureSetup.AllFields = true;
            this.pEnumFeature = (IEnumFeature)iEnumFeatureSetup;
            this.pEnumFeature.Reset();

            IFeature pFeature = pEnumFeature.Next();
            if (pFeature == null) return;
            while (pFeature != null)
            {
                IFeatureLayer pFeatureLayer = mLayer as IFeatureLayer;
                string name = pFeatureLayer.FeatureClass.OIDFieldName;
                if (!pFeature.HasOID)
                {
                    pFeature = pEnumFeature.Next();
                    return;
                }
                int ID = pFeature.OID;
                string sqlWhere = "";
                sqlWhere = name + "=" + ID;

                this.pItems.Add(sqlWhere);
                comboBox1.Items.Add(sqlWhere);
                pFeature = pEnumFeature.Next();


            }

            comboBox1.Text = pItems[0];
        }
    }
}
