using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EngineForms.Command
{
    class EditSplitCommand : BaseTool
    {
        private IHookHelper m_hookHelper;
        private IFeature pFeature;
        public ILayer mLayer;
        public AxMapControl pMapControl;
        public IEngineEditor mEngineEditor;
        public IMap mMap;

        public EditSplitCommand()
        {
        }

        public override void OnCreate(object hook)
        {
            try
            {
                m_hookHelper = new HookHelperClass();
                m_hookHelper.Hook = hook;
                base.m_enabled = true;
            }
            catch
            {
                m_hookHelper = null;
            }
        }




        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button != 1)

                return;



            #region……分割面

            //根据已选择的要分割的要素的类型绘制分割线

            if (((IFeatureLayer)mLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)

            {//分割线的样式

                IScreenDisplay pScreenDisplay = pMapControl.ActiveView.ScreenDisplay;

                ISimpleLineSymbol pLineSymbol = new SimpleLineSymbolClass();

                IRgbColor pRgbColor = new RgbColorClass();

                pRgbColor.Red = 255;

                pLineSymbol.Color = pRgbColor;

                IRubberBand pRubberBand = new RubberLineClass();

                IPolyline pPolyline = (IPolyline)pRubberBand.TrackNew(pScreenDisplay, (ISymbol)pLineSymbol);

                pScreenDisplay.StartDrawing(pScreenDisplay.hDC, (short)esriScreenCache.esriNoScreenCache);

                pScreenDisplay.SetSymbol((ISymbol)pLineSymbol);

                pScreenDisplay.DrawPolyline(pPolyline);

                pScreenDisplay.FinishDrawing();



                //清理将被分割的要素

                ITopologicalOperator pTopoOpo;

                pTopoOpo = pPolyline as ITopologicalOperator;

                pTopoOpo.Simplify();//确保几何体的拓扑正确

                IFeatureLayer featureLayer = mLayer as IFeatureLayer;
                IFeatureClass featureClass = featureLayer.FeatureClass;
                IDataset dataset = featureClass as IDataset;
                IWorkspace workspace = dataset.Workspace;
                mEngineEditor.EditSessionMode = esriEngineEditSessionMode.esriEngineEditSessionModeVersioned;
                mEngineEditor.StartEditing(workspace, mMap);
                ((IEngineEditLayers)mEngineEditor).SetTargetLayer(featureLayer, 0);

                //mEngineEditor.StartOperation();

                //分割方法               


                SplitPolygon(featureClass, pPolyline);

                ReBackStates();//刷新返回修改工具

                pMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, pMapControl.ActiveView.Extent);
                //  mEngineEditor.StopEditing(true);

            }

            #endregion



            #region……鼠标画线分割线

            //根据分割要素的类型绘制分割线

            if (((IFeatureLayer)mLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)

            {

                IScreenDisplay pScreenDisplay = pMapControl.ActiveView.ScreenDisplay;

                ISimpleLineSymbol pLineSymbol = new SimpleLineSymbolClass();

                IRgbColor pRgbColor = new RgbColorClass();

                pRgbColor.Red = 255;

                pLineSymbol.Color = pRgbColor;

                IRubberBand pRubberBand = new RubberLineClass();

                IPolyline pPolyline = (IPolyline)pRubberBand.TrackNew(pScreenDisplay, (ISymbol)pLineSymbol);

                pScreenDisplay.StartDrawing(pScreenDisplay.hDC, (short)esriScreenCache.esriNoScreenCache);

                pScreenDisplay.SetSymbol((ISymbol)pLineSymbol);

                pScreenDisplay.DrawPolyline(pPolyline);

                pScreenDisplay.FinishDrawing();



                // mEngineEditor.StartOperation();//开启编辑

                IFeatureLayer featureLayer = mLayer as IFeatureLayer;
                IFeatureClass featureClass = featureLayer.FeatureClass;
                IDataset dataset = featureClass as IDataset;
                IWorkspace workspace = dataset.Workspace;
                mEngineEditor.EditSessionMode = esriEngineEditSessionMode.esriEngineEditSessionModeVersioned;
                mEngineEditor.StartEditing(workspace, mMap);
                ((IEngineEditLayers)mEngineEditor).SetTargetLayer(featureLayer, 0);

                ISelectionSet pSelectionSet = featureClass.Select(null, esriSelectionType.esriSelectionTypeIDSet, esriSelectionOption.esriSelectionOptionNormal, workspace);

                //分割方法

                SplitPolyline(pSelectionSet, pPolyline);



                ReBackStates();

                pMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, pMapControl.ActiveView.Extent);

            }

            #endregion

            mEngineEditor.StopEditing(true);
            //mEngineEditor.StopOperation("ControlToolsEditing_CreateNewFeatureTask");
        }

        //分割面

        public void SplitPolygon(IFeatureClass pFeatureClass, IGeometry pGeometry)

        {

            //使用空间过滤器来获得将要与线或点进行分割的要素类

            IFeatureCursor pFeatCursor;

            ICursor pCursor;

            ISpatialFilter pSpatialFilter;

            pSpatialFilter = new SpatialFilterClass();

            pSpatialFilter.Geometry = pGeometry;

            if (pGeometry.GeometryType == esriGeometryType.esriGeometryPolyline)//.esriGeometryPoint)

            {

                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;//空间关系

            }



            pFeatCursor = pFeatureClass.Search(pSpatialFilter, false);

            //清理将被分割的要素

            ITopologicalOperator pTopoOpo;

            pTopoOpo = pGeometry as ITopologicalOperator;

            pTopoOpo.Simplify();//确保几何体的拓扑正确

            IFeature pFeature;

            pFeature = pFeatCursor.NextFeature();

            if (pFeature == null) return;

            while (pFeature != null)

            {

                IFeatureEdit2 pFeatureEdit;

                pFeatureEdit = pFeature as IFeatureEdit2;

                ISet pSet;

                pSet = pFeatureEdit.Split(pGeometry);//直接用线分割

                for (int setCount = 0; setCount < pSet.Count; setCount++)

                {

                    pFeature = pSet.Next() as IFeature;

                    //featureSelection.SelectionSet.Add(pFeature.OID);

                }

                pFeature = pFeatCursor.NextFeature();

            }

            MessageBox.Show("面分割完毕！继续选择以分割！");

        }



        //分割线，线要素不能用线来分割，得用点来。所以要将分割线与被分割线求交叉点

        public void SplitPolyline(ISelectionSet pSelectionSet, IGeometry pGeometry)

        {

           
            IFeatureClass featureClass = (mLayer as IFeatureLayer).FeatureClass;

            IEnumIDs enumIDs = pSelectionSet.IDs;

            int id = enumIDs.Next();

            while (id != -1)

            {
                pFeature = featureClass.GetFeature(id);
                IGeometry pGeo = pFeature.ShapeCopy;

                //由于在拓扑是需要空间参考一致，所以要将空间参考坐标设置一下。

                pGeo.SpatialReference = pGeometry.SpatialReference;

                ITopologicalOperator pTopoOpo = pGeo as ITopologicalOperator;

                //Intersect（）方法求出来的是MultiPoint，而不是单点

                IPointCollection pPCol = pTopoOpo.Intersect(pGeometry, esriGeometryDimension.esriGeometry0Dimension) as IPointCollection;

                pTopoOpo.Simplify();



                if (pPCol == null)//如果没有相交的，那么点集就为空。

                    return;

                if (pPCol.PointCount == 0)//如果选择的线有一些没有与分割线相交，那么PointCount为0，但是0并不是null。

                {

                    id = enumIDs.Next();//那么就Next（），让他进入下一个回合吧

                    continue;

                }

                IFeatureEdit pFeatureEdit;

                pFeatureEdit = pFeature as IFeatureEdit;

                ISet pSet;

                pSet = pFeatureEdit.Split(pPCol.get_Point(0));

                pSet.Reset();



                //这一步进入分割大赛，其实分割为两个部分，当然要是只是分割单个要素的话就用不着里面那一层嵌套循环了。

                for (int setCount = 0; setCount < pSet.Count; setCount++)

                {

                    pFeature = pSet.Next() as IFeature;

                    if (pFeature == null) return;

                    for (int i = 1; i < pPCol.PointCount; i++)

                    {

                        try

                        {

                            pFeatureEdit = pFeature as IFeatureEdit;

                            IPoint pPoint = pPCol.get_Point(i);

                            pSet = pFeatureEdit.Split(pPoint);//这里新产生的线要素，可能与下一个交点进行分割，所以要重新获取以便进行下一次分割

                            pSet.Reset();

                            pPCol.RemovePoints(i, 1);//为了少循环一次，用了一个点就从点集中移除它，因为两线相交肯定没有二心。

                            break;//打断，这一步是迫不得已的，因为懒得去想另一部分。当然这样的话有个问题：如3点分线应该是4段，但如果这点刚好是中间的点，那么就会丢掉他前面或者后面的一个分割点，如此的话就变成2点分线为3段了；而且画的分割折线与被分割线交点越多丢失的就越多。主要原因是MultiPoint撞到IPointCollection中后顺序并不是画线时候的交叉顺序。怎么办呢？

                        }

                        catch

                        {

                            continue;

                        }

                    }

                }

                id = enumIDs.Next();

            }

            MessageBox.Show("线分割完毕！继续选择以分割！");

        }



        private void ReBackStates()

        {

            //清空选择集

            ICommand pCommand = new ControlsClearSelectionCommandClass();

            pCommand.OnCreate(pMapControl.Object);

            pCommand.OnClick();

            pCommand = new ControlsEditingEditToolClass();

            pCommand.OnCreate(pMapControl.Object);

            pMapControl.CurrentTool = pCommand as ITool;

        }
       
    }
}
