using DevExpress.XtraEditors;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EngineForms.Command
{
    class GeneralizeCommand : BaseTool
    {
        private IHookHelper m_hookHelper;
        public ILayer mLayer;
        public AxMapControl pMapControl;
     

        public override void OnClick()
        {      
            offset mOffset = new offset(mLayer,pMapControl);
            mOffset.Show(pMapControl);
        }

        public override void OnCreate(object hook)
        {
            try
            {
                m_hookHelper = new HookHelperClass();
                m_hookHelper.Hook = hook;
                m_enabled = true;
            }
            catch
            {
                m_hookHelper = null;
            }
        }
    }
}
