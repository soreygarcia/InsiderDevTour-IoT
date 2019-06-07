using System.IO;
using System.Threading.Tasks;
using InsiderDevTour.Sensors.Constants;
using Windows.Storage;
using System;
using System.Linq;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.Collections.Generic;

namespace InsiderDevTour.Sensors.Services
{
    public static class FaceAPI
    {
        // Cognitive Services Face API client
        private static FaceClient faceServiceClient = new FaceClient(
                new ApiKeyServiceClientCredentials(AzureCredentials.subscriptionKey),
                new System.Net.Http.DelegatingHandler[] { })
        {
            Endpoint = AzureCredentials.serviceEndpoint
        };  // need to provide and endpoint and a delegate.

        public static async Task<string> GetViewerName(string url)
        {
            // generic placeholder for viewer's name
            string personName = "friend";

            try
            {
                // ask Face API to detect faces in photo 
                var faces = await faceServiceClient.Face.DetectWithUrlAsync(url, true, false);

                if (faces.Count > 0)
                {
                    // convert result to array of id's to identify
                    var faceIds = faces.Where(face => face.FaceId != null).Select(face => face.FaceId.Value).ToList();

                    // ask face API to identify face from a trained person group and the id's we got back
                    var results = await faceServiceClient.Face.IdentifyAsync(faceIds, AzureCredentials.personGroup);

                    // go through the returned identification results from the Face API
                    foreach (IdentifyResult identifyResult in results)
                    {
                        // if we have a prediction for who they are
                        if (identifyResult.Candidates.Count > 0)
                        {
                            Guid candidateId = identifyResult.Candidates[0].PersonId;
                            // extract the candidate's name using their ID
                            Person person = await faceServiceClient.PersonGroupPerson.GetAsync(AzureCredentials.personGroup, candidateId);
                            personName = person.Name;
                        }
                    }
                }

                return personName;
            }
            catch (Exception e)
            {
                return "strange";
            }
        }

        public static async Task<string> GetViewerName(StorageFile photoFile)
        {
            // generic placeholder for viewer's name
            string personName = "friend";

            try
            {
                using (FileStream photoStream = new FileStream(photoFile.Path, FileMode.Open))
                {
                    // ask Face API to detect faces in photo 
                    var faces = await faceServiceClient.Face.DetectWithStreamAsync(photoStream, true, false);

                    if (faces.Count > 0)
                    {
                        // convert result to array of id's to identify
                        var faceIds = faces.Where(face => face.FaceId != null).Select(face => face.FaceId.Value).ToList();

                        // ask face API to identify face from a trained person group and the id's we got back
                        var results = await faceServiceClient.Face.IdentifyAsync(faceIds, AzureCredentials.personGroup);

                        // go through the returned identification results from the Face API
                        foreach (IdentifyResult identifyResult in results)
                        {
                            // if we have a prediction for who they are
                            if (identifyResult.Candidates.Count > 0)
                            {
                                Guid candidateId = identifyResult.Candidates[0].PersonId;
                                // extract the candidate's name using their ID
                                Person person = await faceServiceClient.PersonGroupPerson.GetAsync(AzureCredentials.personGroup, candidateId);
                                personName = person.Name;
                            }
                        }
                    }

                    return personName;
                }
            }
            catch (Exception e)
            {
                return "strange";
            }
        }
    }
}