using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.Carto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineForms.Command
{
    class MergeCommand : BaseTool
    {
        private IHookHelper m_hookHelper;

        public ILayer mLayer;
        public AxMapControl pMapControl;

        public MergeCommand()
        {
        }
        public MergeCommand(ILayer mLayer)
        {
            this.mLayer = mLayer;
        }

        public override void OnClick()
        {
            MergeForm margeForm = new MergeForm(mLayer, pMapControl);
            margeForm.Show(pMapControl);
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
