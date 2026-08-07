using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.Opertion.LoadPort
{
  public class Message
  {

        private string msg;

        private string cmd;

        private string param;

        public string Msg { get => msg; set => msg = value; }
        public string Cmd { get => cmd; set => cmd = value; }
        public string Param { get => param; set => param = value; }

        public Message(string msg,string cmd ,string param)
        {
            Msg = msg;
            Cmd = cmd;
            Param = param;
        }
    }
}
