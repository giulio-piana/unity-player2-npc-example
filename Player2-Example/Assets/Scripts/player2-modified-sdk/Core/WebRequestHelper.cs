using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Player2ModifiedSdk
{
    public class WebRequestHelper
    {

        public static async Task SendWebRequestAsync(string requestUrl, string RequestBodyJson, Action<string> onResponseSuccess, Action<string> onResponseError, CancellationToken cancellationToken)
        {
            Debug.Log("Sending async request: " + RequestBodyJson);

            using (UnityWebRequest request = new UnityWebRequest(requestUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(RequestBodyJson);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");
               // request.SetRequestHeader("Accept", "application/json");

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    // Check if the cancellation token has been triggered
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Debug.LogWarning("Request canceled.");
                        onResponseError?.Invoke("Request canceled.");
                        request.Abort(); // Abort the request
                        return; // Exit the method
                    }

                    await System.Threading.Tasks.Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Async chat request failed: " + request.error);
                    onResponseError?.Invoke("Error: " + request.error);
                }
                else
                {
                    string jsonResponse = request.downloadHandler.text;

                    Debug.Log("Async chat response: " + jsonResponse);

                    onResponseSuccess?.Invoke(jsonResponse);

                }
            }
        }
    }
}