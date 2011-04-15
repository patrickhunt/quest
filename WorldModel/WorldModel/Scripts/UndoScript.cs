﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class UndoScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "undo"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new UndoScript(WorldModel);
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 0 }; }
        }
    }

    public class UndoScript : ScriptBase
    {
        private WorldModel m_worldModel;

        public UndoScript(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        public override void Execute(Context c)
        {
            m_worldModel.UndoLogger.Undo();
        }

        public override string Save()
        {
            return "undo";
        }

        public override string Keyword
        {
            get
            {
                return "undo";
            }
        }

        public override object GetParameter(int index)
        {
            throw new ArgumentOutOfRangeException();
        }

        public override void SetParameterInternal(int index, object value)
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    public class StartTransactionConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "start transaction"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new StartTransactionScript(WorldModel, new Expression<string>(parameters[0], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class StartTransactionScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunction<string> m_command;

        public StartTransactionScript(WorldModel worldModel, IFunction<string> command)
        {
            m_worldModel = worldModel;
            m_command = command;
        }

        public override void Execute(Context c)
        {
            m_worldModel.UndoLogger.StartTransaction(m_command.Execute(c));
        }

        public override string Save()
        {
            return SaveScript("start transaction", m_command.Save());
        }

        public override string Keyword
        {
            get
            {
                return "start transaction";
            }
        }

        public override object GetParameter(int index)
        {
            return m_command.Save();
        }

        public override void SetParameterInternal(int index, object value)
        {
            m_command = new Expression<string>((string)value, m_worldModel);
        }
    }

    public class EndTransactionConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "end transaction"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new EndTransactionScript(WorldModel);
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 0 }; }
        }
    }

    public class EndTransactionScript : ScriptBase
    {
        private WorldModel m_worldModel;

        public EndTransactionScript(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        public override void Execute(Context c)
        {
            m_worldModel.UndoLogger.EndTransaction();
        }

        public override string Save()
        {
            return "end transaction";
        }

        public override string Keyword
        {
            get
            {
                return "end transaction";
            }
        }

        public override object GetParameter(int index)
        {
            throw new ArgumentOutOfRangeException();
        }

        public override void SetParameterInternal(int index, object value)
        {
            throw new ArgumentOutOfRangeException();
        }
    }
}
