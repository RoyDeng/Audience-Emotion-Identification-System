using System.Linq;
using XSockets.Core.XSocket;
using XSockets.Core.XSocket.Helpers;
using XSockets.Core.Common.Socket.Event.Interface;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Emotion;
using XSockets.Core.XSocket.Model;

namespace AEIS.RTC
{
    public class Emotion : XSocketController
    {
        private const string subscriptionKey = "cfacac020ce2482cb6fe4cfad5f2be99";  //API金鑰
        private static EmotionServiceClient emotionServiceClient;

        static Emotion()
        {
            emotionServiceClient = new EmotionServiceClient(subscriptionKey);
        }

        //偵測表情的執行緒
        public async Task DetectEmotion(IMessage message)
        {
            using (var ms = new System.IO.MemoryStream(message.Blob.ToArray()))
            {
                var emotionResult = await emotionServiceClient.RecognizeAsync(ms);
                var bm = new Message(message.Blob.ToArray(), emotionResult, "emotionDetected", "emotion");
                await this.InvokeToAll(bm);
            }
        }

        public override async Task OnMessage(IMessage message)
        {
            await this.InvokeToAll(message);
        }
    }
}