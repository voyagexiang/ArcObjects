using DevExpress.XtraEditors;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
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
    public partial class AttributeTable : DevExpress.XtraEditors.XtraForm
    {
        ILayer mLayer;
        AxMapControl mAxMapControl1;
        AxMapControl mAxMapControl2;
        HashSet<int> mDataGridTableChanged = new HashSet<int>();
        IMap mMap;
        public AttributeTable(ILayer layer, AxMapControl axMapControl)
        {
            InitializeComponent();
            mLayer = layer;
            mAxMapControl1 = axMapControl;
            mMap = this.mAxMapControl1.Map;

        }
        public AttributeTable(ILayer layer, AxMapControl axMapControl1, AxMapControl axMapControl2)
        {
            InitializeComponent();
            mLayer = layer;
            mAxMapControl1 = axMapControl1;
            mAxMapControl2 = axMapControl2;
            mMap = this.mAxMapControl1.Map;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> sqlWheres = GetSqlWhere();
            if (sqlWheres == null) return;
            DialogResult result = DialogResult.OK;
            if (sqlWheres.Count == 0)
            {
                result = XtraMessageBox.Show("请选择要素", "提示信息", MessageBoxButtons.OKCancel);
                return;
            }

            result = XtraMessageBox.Show("确定删除吗？", "删除提示", MessageBoxButtons.OKCancel);
            if (result.ToString() != "OK") return;

            IFeatureLayer pFeatureLayer = mLayer as IFeatureLayer;
            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;

            //启动编辑
            IFeatureDataset mFeatureDataset = pFeatureClass.FeatureDataset;
            IWorkspace workspace = null;
            if (mFeatureDataset == null)
            {
                workspace = pFeatureClass as Workspace;
            }
            else
            {
                workspace = pFeatureClass.FeatureDataset.Workspace;
            }
            IEngineEditor mEngineEditor = new EngineEditorClass();
            if (workspace != null)
            {

                //mEngineEditor.EditSessionMode = esriEngineEditSessionMode.esriEngineEditSessionModeNonVersioned;
                mEngineEditor.EditSessionMode = esriEngineEditSessionMode.esriEngineEditSessionModeVersioned;

                mEngineEditor.StartEditing(workspace, mMap);
                ((IEngineEditLayers)mEngineEditor).SetTargetLayer(pFeatureLayer, -1);
                mEngineEditor.StartOperation();
            }



            mAxMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, mLayer, null);

            try
            {

                for (int i = 0; i < sqlWheres.Count; i++)
                {
                    delectSelectedCells(sqlWheres[i], pFeatureClass);
                }
                if (workspace != null)
                {
                    //保存编辑
                    mEngineEditor.StopEditing(true);
                }

                //删除属性表
                deleteTableRows(sqlWheres);

                mAxMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, mLayer, null);
                if (mAxMapControl2 != null) { mAxMapControl2.Refresh(); }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<string> GetSqlWhere()
        {
            DataGridViewSelectedRowCollection selectedRows = dataGridView1.SelectedRows;
            List<string> queryList = new List<string>();
            //获取表头字段名称
            DataGridViewColumnCollection headers = dataGridView1.Columns;
            List<string> HeadersFields = new List<string>();

            for (int i = 0; i < headers.Count; i++)
            {
                HeadersFields.Add(headers[i].HeaderText);
            }

            if (selectedRows.Count == 0) { }
            else
            {
                for (int i = 0; i < selectedRows.Count; i++)
                {
                    string sqlWhere = "";

                    if (HeadersFields.Contains("FID"))
                    {
                        if (selectedRows[i].Cells["FID"].EditedFormattedValue.ToString() == "")
                        {
                            XtraMessageBox.Show("请选择有效要素", "提示信息", MessageBoxButtons.OK);
                            return null;
                        }
                        sqlWhere += "FID=" + selectedRows[i].Cells["FID"].EditedFormattedValue;
                    }
                    if (HeadersFields.Contains("OBJECTID"))
                    {
                        if (selectedRows[i].Cells["OBJECTID"].EditedFormattedValue.ToString() == "")
                        {
                            XtraMessageBox.Show("请选择有效要素", "提示信息", MessageBoxButtons.OK);
                            return null;
                        }
                        if (sqlWhere != "")
                        {
                            sqlWhere += "AND OBJECTID=" + selectedRows[i].Cells["OBJECTID"].EditedFormattedValue;
                        }
                        else
                        {
                            sqlWhere += "OBJECTID=" + selectedRows[i].Cells["OBJECTID"].EditedFormattedValue;
                        }

                    }
                    if (HeadersFields.Contains("OID"))
                    {
                        if (selectedRows[i].Cells["OID"].EditedFormattedValue.ToString() == "")
                        {
                            XtraMessageBox.Show("请选择有效要素", "提示信息", MessageBoxButtons.OK);
                            return null;
                        }
                        if (sqlWhere != "")
                        {
                            sqlWhere += "AND OID=" + selectedRows[i].Cells["OID"].EditedFormattedValue;

                        }
                        else
                        {
                            sqlWhere += "OID=" + selectedRows[i].Cells["OID"].EditedFormattedValue;
                        }
                    }
                    queryList.Add(sqlWhere);
                }
            }
            return queryList;
        }

        /// <summary>
        /// 表选删除后，表更新
        /// </summary>
        /// <param name="items"></param>
        private void deleteTableRows(List<string> items)
        {
            if (items.Count == 0) return;
            DataGridViewSelectedRowCollection selectedRows = dataGridView1.SelectedRows;
            for (int i = 0; i < selectedRows.Count; i++)
            {

                dataGridView1.Rows.Remove(selectedRows[i]);
            }

        }

        private void delectSelectedCells(string strWhereClause, IFeatureClass pFeatureClass)
        {


            IQueryFilter pQueryFilter = new QueryFilterClass();
            pQueryFilter.WhereClause = strWhereClause;
            ITable pTable = pFeatureClass as ITable;
            pTable.DeleteSearchedRows(pQueryFilter);

        }

        private void AttributeTable_Load(object sender, EventArgs e)
        {
            showLayerAttributeTable();

        }

        /// <summary>
        /// 属性展示
        /// </summary>
        private void showLayerAttributeTable()
        {
            IFeatureLayer pFeatureLayer = mLayer as IFeatureLayer;
            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
            DataTable dt = new DataTable();
            if (pFeatureLayer != null)
            {
                DataColumn dc;
                for (int i = 0; i < pFeatureClass.Fields.FieldCount; i++)
                {
                    dc = new DataColumn(pFeatureClass.Fields.get_Field(i).Name);
                    dt.Columns.Add(dc);//获取所有列的属性值
                }
                IFeatureCursor pFeatureCursor = pFeatureClass.Search(null, false);
                IFeature pFeature = pFeatureCursor.NextFeature();
                DataRow dr;
                while (pFeature != null)
                {
                    dr = dt.NewRow();
                    for (int j = 0; j < pFeatureClass.Fields.FieldCount; j++)
                    {
                        if (pFeature.Fields.get_Field(j).Name == "Shape")
                        {
                            if (pFeature.Shape.GeometryType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint)
                            {
                                dr[j] = "点";
                            }
                            if (pFeature.Shape.GeometryType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline)
                            {
                                dr[j] = "线";
                            }
                            if (pFeature.Shape.GeometryType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon)
                            {
                                dr[j] = "面";
                            }
                        }
                        else
                        {
                            dr[j] = pFeature.get_Value(j).ToString();//增加行
                        }
                    }
                    dt.Rows.Add(dr);
                    pFeature = pFeatureCursor.NextFeature();

                }
                dataGridView1.DataSource = dt;
            }

        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            mAxMapControl1.Map.ClearSelection();

            IFeatureLayer mFeatureLayer = mLayer as IFeatureLayer;
            IFeatureClass mFeatureClass = mFeatureLayer.FeatureClass;


            List<string> strWhereOrg = GetSqlWhere();
            if (strWhereOrg == null)
            {
                return;
            }
            string strWhere = getQueryString(strWhereOrg);
            if (strWhere == "") return;

            // 属性过滤
            IQueryFilter pQueryFilter = new QueryFilter();

            pQueryFilter.WhereClause = strWhere;




            // 要素游标

            IFeatureCursor mFeatureCursor = mFeatureClass.Search(pQueryFilter, false);
            IFeature mFeatur = mFeatureCursor.NextFeature();


            while (mFeatur != null)
            {

                mAxMapControl1.Map.SelectFeature(mLayer as ILayer, mFeatur);
                //mAxMapControl1.FlashShape(mFeatur.Shape);
                mFeatur = mFeatureCursor.NextFeature();
            }
            mAxMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
        }

        //查询字符串  
        string getQueryString(List<string> strWhereOrg)
        {
            strWhereOrg.Sort();
            string strWhere = "";
            for (int i = 0; i < strWhereOrg.Count; i++)
            {
                if (i != strWhereOrg.Count - 1)
                {
                    strWhere += strWhereOrg[i] + " OR ";
                }
                else
                {
                    strWhere += strWhereOrg[i];
                }
            }
            return strWhere;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            save_attribute();
        }
        private void save_attribute()
        {
            //判断属性有没有改变
            if (mDataGridTableChanged.Count == 0)
            {
                XtraMessageBox.Show("未进行修改", "提示信息", MessageBoxButtons.OK);
                return;
            }

            DataTable dt = dataGridView1.DataSource as DataTable;

            IFeatureLayer pFeatureLayer = mLayer as IFeatureLayer;
            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;

            //启动编辑
            //todo 
            //workspace maybe null
            IWorkspace workspace = pFeatureClass.FeatureDataset.Workspace;
            IEngineEditor mEngineEditor = mEngineEditor = new EngineEditorClass();
            //mEngineEditor.EditSessionMode = esriEngineEditSessionMode.esriEngineEditSessionModeNonVersioned;
            mEngineEditor.EditSessionMode = esriEngineEditSessionMode.esriEngineEditSessionModeVersioned;

            mEngineEditor.StartEditing(workspace, mAxMapControl1.Map);
            ((IEngineEditLayers)mEngineEditor).SetTargetLayer(pFeatureLayer, -1);
            mEngineEditor.StartOperation();

            UpdateFTOnDV(mLayer, dt, mDataGridTableChanged.ToArray<int>());
            mEngineEditor.StopEditing(true);
            mDataGridTableChanged.Clear();
        }

        public void UpdateFTOnDV(ILayer player, DataTable pdatatable, int[] array)
        {


            IFeatureLayer pFeature = player as IFeatureLayer;
            ITable pTable = pFeature.FeatureClass as ITable;
            string mOID = pTable.OIDFieldName;


            ICursor pCursor;
            IRow pRow;

            pCursor = pTable.GetRows(array, false);
            for (int i = 0; i < array.Length; i++)
            {
                pRow = pCursor.NextRow();
                int k = array[i];
                int g = -1;
                for (int l = 0; l < pdatatable.Rows.Count; l++)
                {
                    if (pdatatable.Rows[l][mOID].ToString() == k.ToString())
                    {
                        g = l;
                        break;
                    }
                }

                for (int j = 0; j < pdatatable.Columns.Count; j++)
                {
                    object pgridview = pdatatable.Rows[g][j];
                    object prow = pRow.get_Value(j);

                    if (prow.ToString() != pgridview.ToString())
                    {

                        if (pgridview.ToString() == "点" || pgridview.ToString() == "线" || pgridview.ToString() == "面")
                        {

                        }
                        else
                        {
                            try
                            {
                                pRow.set_Value(j, pgridview);
                                pRow.Store();
                            }
                            catch (Exception ex)
                            {
                                XtraMessageBox.Show("请输入有效值", "提示信息", MessageBoxButtons.OK);
                                pdatatable.Rows[g][j] = pRow.get_Value(j);
                                return;
                            }

                        }
                    }

                }

            }

            MessageBox.Show("数据保存成功！");
        }

        private void AttributeTable_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (mDataGridTableChanged.Count != 0)
            {
                DialogResult result = XtraMessageBox.Show("是否保存修改", "提示信息", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    save_attribute();
                }
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            IFeatureLayer pFTClass = mLayer as IFeatureLayer;
            ITable pTable = pFTClass as ITable;
            if (pTable.HasOID)
            {
                string mOID = pTable.OIDFieldName;
                int id = int.Parse(dataGridView1.CurrentRow.Cells[mOID].Value.ToString());
                mDataGridTableChanged.Add(id);
            }
        }
    }
}
