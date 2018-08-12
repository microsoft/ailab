using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;

namespace Sketch2Code.AI.Management
{
    public class CustomVisionManager
    {
        CustomVisionClient _apiClient;
        public CustomVisionManager(CustomVisionClient client)
        {
            _apiClient = client;

        }

        public async Task<Project> CreateProject(string projectName)
        {
            var project = await _apiClient.TrainingApi.CreateProjectAsync(projectName);
            return project;
        }

        public async Task<Project> GetProject(string projectName)
        {
            var projects = await _apiClient.TrainingApi.GetProjectsAsync();
            var project = projects?.SingleOrDefault(p => p.Name.Equals(projectName, StringComparison.CurrentCultureIgnoreCase));
            return project;
        }

        public async Task<Tag> CreateTag(string tagName, Project project)
        {
            if (project == null)
                throw new ArgumentNullException("project");
            var tag = await _apiClient.TrainingApi.CreateTagAsync(project.Id, tagName);
            return tag;
        }

        public async Task<IList<Tag>> GetTags(Project project)
        {
            try
            {
                if (project == null)
                    throw new ArgumentNullException("project");
                var tags = await _apiClient.TrainingApi.GetTagsAsync(project.Id);
                return tags;
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }

        public async Task<IList<Image>> GetImagesForTraining(Project project, Iteration iteration = null)
        {
            int defaultPageSize = 50;
            int offset = 0;
            bool hasRecords = true;
            var list = new List<Image>();

            while (hasRecords)
            {
                var images = await _apiClient.TrainingApi.GetTaggedImagesAsync(project.Id, iteration?.Id, skip: offset * defaultPageSize);
                offset += 1;
                hasRecords = images.Count > 0;
                list.AddRange(images);
            }

            return list;
        }

        public async Task CreateDataset(Project project, IList<Image> imageDataset)
        {
            //Get Tags
            var tags = await this.GetTags(project);
            if (tags!=null && !tags.Any() || tags==null)
            {
                //Create Tags
                var imageTags = imageDataset.SelectMany(i => i.Tags).GroupBy(i => i.TagName).Select(g => g.Key).Distinct();
                foreach (var tag in imageTags)
                {
                    var newTag = await this._apiClient.TrainingApi.CreateTagAsync(project.Id, tag);
                    tags.Add(newTag);
                }
            }
            
            //Set Regions with created tags
            var entries = new List<ImageFileCreateEntry>();

            //Create Images With Regions And Tags            
            foreach (var img in imageDataset)
            {
                //download image as byte array 
                using (var client = new WebClient())
                {
                    var buffer = await client.DownloadDataTaskAsync(img.ImageUri);
                    var regions = img.Regions.ToList().ConvertAll<Region>((imageRegion) =>
                    {
                        var tag = tags.SingleOrDefault((t) => t.Name.Equals(imageRegion.TagName, StringComparison.InvariantCultureIgnoreCase));
                        return new Region
                        {
                            Height = imageRegion.Height,
                            Left = imageRegion.Left,
                            Top = imageRegion.Top,
                            Width = imageRegion.Width,
                            TagId = tag.Id
                        };
                    });
                    entries.Add(new ImageFileCreateEntry
                    {
                        Contents = buffer,
                        Regions = regions,
                        TagIds = regions.Select(r => r.TagId).ToList()
                    });
                }
            }


            //Custom vision only allows a max of 64 items as batch size
            int batchSize = 64;

            var pagedEntries = entries
                .Select((e, index) => new { Item = e, Index = index })
                .GroupBy(e=>e.Index/batchSize).ToList();

            foreach (var entryList in pagedEntries)
            {
                _apiClient.TrainingApi.CreateImagesFromFiles(project.Id, new ImageFileCreateBatch
                {
                    Images = entryList.Select(e=>e.Item).ToList(),
                    TagIds = entryList.SelectMany(e => e.Item.TagIds).Distinct().ToList()
                });
            }
            
        }

    }
}
