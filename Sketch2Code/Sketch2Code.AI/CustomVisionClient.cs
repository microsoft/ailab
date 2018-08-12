
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Microsoft.ProjectOxford.Vision;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sketch2Code.AI
{
    public abstract class CustomVisionClient
    {
        protected string _trainingApiKey;
        protected string _predictionApiKey;
        private TrainingApi _trainingApi;
        protected string _projectName;
        protected Project _project;
        protected Iteration _iteration;
        protected VisionServiceClient _visionClient;

        public TrainingApi TrainingApi { get => _trainingApi;  }

        public CustomVisionClient(string trainingKey, string predictionKey, string projectName)
        {
            _trainingApiKey = trainingKey;
            _predictionApiKey = predictionKey;
            _projectName = projectName;
            this._trainingApi = new TrainingApi() { ApiKey = _trainingApiKey };

            _visionClient = new VisionServiceClient(ConfigurationManager.AppSettings["HandwrittenTextSubscriptionKey"],
                ConfigurationManager.AppSettings["HandwrittenTextApiEndpoint"]);
        }
        
        protected async Task<Project> GetProjectAsync(string projectName)
        {
            if (String.IsNullOrWhiteSpace(projectName)) throw new ArgumentNullException("projectName");
            var projects = await this._trainingApi.GetProjectsAsync();

            return projects.SingleOrDefault(p => p.Name.Equals(projectName, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual void Initialize()
        {
            if (String.IsNullOrWhiteSpace(_projectName)) throw new ArgumentNullException("projectName");
            var projects = this._trainingApi.GetProjects();

            this._project = projects.SingleOrDefault(p => p.Name.Equals(_projectName, StringComparison.InvariantCultureIgnoreCase));

            if (_project == null) throw new InvalidOperationException($"CustomVision client failed to initialize. ({_projectName} Not Found.)");


            SetDefaultIteration(ConfigurationManager.AppSettings["ObjectDetectionIterationName"]);
        }

        protected async Task<IList<Iteration>> GetIterations(string projectName)
        {
            if (String.IsNullOrWhiteSpace(projectName)) throw new ArgumentNullException("projectName");

            var prj = await this.GetProjectAsync(projectName);

            var iterations = await this._trainingApi.GetIterationsAsync(prj.Id);

            return iterations;
        }

        public virtual async Task SetDefaultIterationAsync(string iterationName)
        {
            if (_project == null) throw new InvalidOperationException("Project is null");

            var iterations = await this._trainingApi.GetIterationsAsync(_project.Id);

            var iteration = iterations.SingleOrDefault(i=>i.Name == iterationName);

            if (iteration == null) throw new InvalidOperationException($"Iteration {iterationName} not found");

            iteration.IsDefault = true;

            await _trainingApi.UpdateIterationAsync(_project.Id, iteration.Id, iteration);
        }

        public virtual void SetDefaultIteration(string iterationName)
        {
            if (_project == null) throw new InvalidOperationException("Project is null");

            var iterations = this._trainingApi.GetIterations(_project.Id);

            var iteration = iterations.SingleOrDefault(i => i.Name == iterationName);

            if (iteration == null) throw new InvalidOperationException($"Iteration {iterationName} not found");

            iteration.IsDefault = true;

            this._trainingApi.UpdateIteration(_project.Id, iteration.Id, iteration);
        }
    }
}
