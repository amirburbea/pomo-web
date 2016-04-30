using System;
using System.Diagnostics;
using System.ServiceModel;

namespace PoMo.Common.ServiceModel
{
    public static class ServiceModelMethods
    {
        public static void TryClose(object obj)
        {
            try
            {
                ICommunicationObject communicationObject = obj as ICommunicationObject;
                if (communicationObject == null)
                {
                    return;
                }
                switch (communicationObject.State)
                {
                    case CommunicationState.Faulted:
                    case CommunicationState.Closed:
                    case CommunicationState.Closing:
                        return;
                }
                communicationObject.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}