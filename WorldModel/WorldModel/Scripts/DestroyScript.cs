﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class DestroyScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "destroy"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new DestroyScript(WorldModel, new Expression<string>(parameters[0]));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
        #endregion
    }

    public class DestroyScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunction<string> m_expr;

        public DestroyScript(WorldModel worldModel, IFunction<string> expr)
        {
            m_worldModel = worldModel;
            m_expr = expr;
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            m_worldModel.ObjectFactory.DestroyObject(m_expr.Execute(c));
        }

        public override string Save()
        {
            return SaveScript("destroy", m_expr.Save());
        }

        #endregion
    }
}
