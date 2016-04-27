using System;
using System.Diagnostics;
using System.ServiceModel;

namespace PoMo.Common.ServiceModel
{
    public static class ServiceModelMethods
    {
        public static void TryDispose(object obj)
        {
            if (obj == null)
            {
                return;
            }
            try
            {
                ICommunicationObject communicationObject;
                if ((communicationObject = obj as ICommunicationObject) != null &&
                    (communicationObject.State == CommunicationState.Faulted || communicationObject.State == CommunicationState.Closed))
                {
                    return;
                }
                IDisposable disposable = obj as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                    return;
                }
                communicationObject?.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}