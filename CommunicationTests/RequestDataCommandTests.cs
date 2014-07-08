using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommunicationTests
{
    [TestClass]
    public class RequestDataCommandTests
    {
        const int BAUDRATE = 9600;
        bool respondRecieved;

        //[TestMethod]
        //public void RequestCommandTest()
        //{
        //    respondRecieved = false;

        //    PDI.Communication.Port prt = new PDI.Communication.Port("COM4", BAUDRATE);
        //    var timeStamp = DateTime.Now;
        //    while (timeStamp + TimeSpan.FromSeconds(5) > DateTime.Now & !respondRecieved) ;//wait arduino ready

        //    PDI.Communication.RequestExperimentStateCommand cmd = new PDI.Communication.RequestExperimentStateCommand();
        //    cmd.RespondRecieved += cmd_RespondRecieved;
        //    prt.SendCommand(cmd);

        //    timeStamp = DateTime.Now;
        //    while(timeStamp + TimeSpan.FromSeconds(2) > DateTime.Now & !respondRecieved) ;
        //    if (!respondRecieved)
        //        Assert.Fail("Ответ не получен");
        //}

        void cmd_RespondRecieved(object sender, PDI.Communication.ExperimentStateRecievedEventArgs eventArgs)
        {
            respondRecieved = true;
        }
    }
}
