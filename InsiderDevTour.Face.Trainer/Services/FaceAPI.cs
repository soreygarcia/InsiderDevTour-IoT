using InsiderDevTour.Face.Trainer.Constants;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace InsiderDevTour.Face.Trainer.Services
{
    class FaceAPI
    {
        // Cognitive Services Face API client
        private static FaceClient faceServiceClient = new FaceClient(
                new ApiKeyServiceClientCredentials(AzureCredentials.subscriptionKey),
                new System.Net.Http.DelegatingHandler[] { })
        {
            Endpoint = AzureCredentials.serviceEndpoint
        };  // need to provide and endpoint and a delegate.

        public static async Task<Person> CreatePerson(string personGroupId, string personName)
        {
            return await faceServiceClient.PersonGroupPerson.CreateAsync(
                // Id of the PersonGroup that the person belonged to
                personGroupId,
                // Name of the person
                personName
            );
        }

        public static async Task<IList<Person>> GetPeopleByGroup (string personGroupId)
        {
            return await faceServiceClient.PersonGroupPerson.ListAsync(personGroupId);
        }

        public static async Task<PersistedFace> AddFace(StorageFile photoFile, string personGroupId, Guid personId)
        {
            using (FileStream photoStream = new FileStream(photoFile.Path, FileMode.Open))
            {
                // Detect faces in the image and add to Anna
                return await faceServiceClient.PersonGroupPerson.AddFaceFromStreamAsync(
                    personGroupId, personId, photoStream);
            }
        }

        public static async Task<PersistedFace> AddFace(string photoFile, string personGroupId, Guid personId)
        {
            return await faceServiceClient.PersonGroupPerson.AddFaceFromUrlAsync(
                personGroupId, personId, photoFile);
        }

        public static async Task<string> GetViewerName(StorageFile photoFile, string personGroup)
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
                        var results = await faceServiceClient.Face.IdentifyAsync(faceIds, personGroup);

                        // go through the returned identification results from the Face API
                        foreach (IdentifyResult identifyResult in results)
                        {
                            // if we have a prediction for who they are
                            if (identifyResult.Candidates.Count > 0)
                            {
                                Guid candidateId = identifyResult.Candidates[0].PersonId;
                                // extract the candidate's name using their ID
                                Person person = await faceServiceClient.PersonGroupPerson.GetAsync(personGroup, candidateId);
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
