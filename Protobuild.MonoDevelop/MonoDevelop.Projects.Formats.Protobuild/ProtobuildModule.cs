﻿//
// ProtobuildModule.cs
//
// Author:
//       James Rhodes <jrhodes@redpointsoftware.com.au>
//
// Copyright (c) 2015 James Rhodes
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Projects.Extensions;
using MonoDevelop.Projects.Formats.MSBuild;

namespace MonoDevelop.Projects.Formats.Protobuild
{
	public class ProtobuildModule : Solution, IProtobuildModule
    {
		ProtobuildAppDomain appDomain;

		ProtobuildModuleInfo latestModuleInfo;

        Dictionary<string, Solution> shadowSolutions;

		// TODO We really should have a way of getting these defaults from
		// the Protobuild executable itself.
		public const string DEFAULT_PLATFORMS = "Android,iOS,Linux,MacOS,Ouya,PCL,PSMobile,Windows,Windows8,WindowsGL,WindowsPhone,WindowsPhone81";

	    public ProtobuildModule ()
	    {
            shadowSolutions = new Dictionary<string, Solution> ();
        }

        public string ActiveConfiguration { get; set; }

		public Action AfterSave { get; set; }

		SolutionFolder IProtobuildModule.RootFolder 
		{
			get { return this.RootFolder; }
			set { this.RootFolder = value; }
		}

		public string SupportedPlatformsString
		{
			get
			{
				if (!string.IsNullOrEmpty(latestModuleInfo.SupportedPlatforms))
				{
					return latestModuleInfo.SupportedPlatforms;
				}

				return DEFAULT_PLATFORMS;
			}
		}

		public string[] SupportedPlatformsArray
		{
			get
			{
				return this.SupportedPlatformsString.Split (',');
			}
		}

	    public void Load(string path, IProgressMonitor monitor)
	    {
			if (appDomain != null)
			{
                monitor.BeginStepTask ("Stopping Protobuild...", 1, 1);
				appDomain.UnloadAppDomain();
				appDomain.ProtobuildChangedEvent -= HandleProtobuildChangedEvent;
				appDomain = null;
			}

            monitor.BeginStepTask("Starting Protobuild...", 1, 1);
			appDomain = new ProtobuildAppDomain(path);
			appDomain.ProtobuildChangedEvent += HandleProtobuildChangedEvent;
            latestModuleInfo = appDomain.LoadModule(monitor);

            FileName = path;

            // We have to perform an initial generation on load so we have a set of
            // projects that can be used for the type system.  Also we generate before
            // loading projects because generation might trigger package resolution.
	        ActiveConfiguration = appDomain.HostPlatform;
            EnsureProjectsAreGeneratedForPlatform(appDomain.HostPlatform, monitor);

            monitor.BeginStepTask("Loading projects...", 1, 1);
		    ReloadProjects ();
		}

        public void SaveModule(FilePath file, IProgressMonitor monitor)
        {
            try {
                monitor.BeginTask ("Saving module " + latestModuleInfo.Name + "...", 1);

                if (this.SingleStartup) {
                    latestModuleInfo.DefaultStartupProject = this.StartupItem.Name;
                }

                appDomain.SaveModule (latestModuleInfo, monitor);
            }
            catch (Exception ex) {
                monitor.ReportError ("Failed to save module " + latestModuleInfo.Name, ex);
                throw;
            }
            finally {
                monitor.ReportSuccess ("Saved module " + latestModuleInfo.Name);
                monitor.EndTask ();
            }

			DefinitionOrModuleSaved();
        }

		public void DefinitionOrModuleSaved()
		{
			if (AfterSave != null)
			{
				AfterSave();
			}
		}

		public void OnDefinitionBuilt(ProtobuildDefinition definition)
		{
			if (Built != null)
			{
				DefinitionBuilt(this, new SolutionItemEventArgs(definition, this));
			}
		}

	    public override FileFormat FileFormat
	    {
	        get { return Services.ProjectService.FileFormats.GetFileFormat ("protobuild"); }
	    }

	    void HandleProtobuildChangedEvent (object sender, EventArgs e)
		{
            latestModuleInfo = appDomain.LoadModule(null);

            ReloadProjects();
		}

	    private void ReloadProjects ()
        {
            Configurations.Clear();

			foreach (var platform in SupportedPlatformsArray)
            {
                AddConfiguration(platform, false);
            }

            RootFolder.Items.Clear ();

            LoadSubmodule(this, latestModuleInfo);

            DefaultConfigurationId = appDomain.HostPlatform;
	    }

	    private void EnsureConfigurationPresent (string platform, ProtobuildDefinition item)
	    {
	        var solutionConfig = Configurations[platform];
	        if (solutionConfig == null) {
                solutionConfig = new SolutionConfiguration(platform);
                Configurations.Add(solutionConfig);
	        }

	        var conf = solutionConfig.GetEntryForItem (item);
	        if (conf == null) {
	            solutionConfig.AddItem (item, true, platform);
	        }
	    }

	    private void LoadSubmodule (IProtobuildModule folder, ProtobuildModuleInfo moduleInfo)
        {
	        if (folder.Packages == null) {
	            folder.Packages = new ProtobuildPackages ();
	        }

	        if (folder.Definitions == null) {
	            folder.Definitions = new ItemCollection<ProtobuildDefinition> ();
	        }

	        if (folder.Submodules == null) {
	            folder.Submodules = new ItemCollection<ProtobuildSubmodule> ();
	        }

	        var packagesByFolder = new Dictionary<string, ProtobuildPackage> ();

            foreach (var package in moduleInfo.Packages)
            {
				var packageItem = new ProtobuildPackage(moduleInfo, package, folder);
                //packageItem.SetItemHandler(new MSBuildHandler(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));
                folder.Packages.Add(packageItem);
                packagesByFolder.Add(package.Folder.Replace ('\\','/'), packageItem);
            }

            foreach (var submodule in moduleInfo.LoadedSubmodules) {
                var submoduleRelativeFolder = submodule.Path.Substring (moduleInfo.Path.Length).Trim (new[] {'/', '\\'});
                submoduleRelativeFolder = submoduleRelativeFolder.Replace ('\\', '/');

                if (packagesByFolder.ContainsKey (submoduleRelativeFolder)) {
                    // Place all the projects under the package's folder instead.
                    LoadSubmodule(packagesByFolder[submoduleRelativeFolder], submodule);
                }
                else
                {
					var submoduleItem = new ProtobuildSubmodule(moduleInfo, submodule, this.RootFolder);
                    //submoduleItem.SetItemHandler(new MSBuildHandler(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));
                    folder.Submodules.Add(submoduleItem);

                    LoadSubmodule(submoduleItem, submodule);
                }
            }

            foreach (var definition in moduleInfo.LoadedDefinitions)
            {
                var definitionItem = new ProtobuildDefinition(moduleInfo, definition, folder);
                definitionItem.ParentSolution = this;
                definitionItem.SetItemHandler(new MSBuildHandler(definition.Guid.ToString(), definition.Guid.ToString()));
                folder.Definitions.Add(definitionItem);

				// Add the definition to the root folder as well so that events
				// get hooked up correctly.
				folder.RootFolder.Items.Add(definitionItem);

                foreach (var config in definitionItem.GetConfigurations ()) {
                    EnsureConfigurationPresent (config, definitionItem);
                }

                if (moduleInfo.DefaultStartupProject == definition.Name) {
                    this.SingleStartup = true;
                    this.StartupItem = definitionItem;
                }
            }
	    }

	    public override string Name
	    {
	        get { return latestModuleInfo.Name; }
	        set { throw new NotSupportedException(); }
	    }

	    protected override BuildResult OnBuild (IProgressMonitor monitor, ConfigurationSelector configuration)
        {
            var platform = configuration.GetConfiguration(this).Id;
	        EnsureProjectsAreGeneratedForPlatform (platform, monitor);

	        var solution = shadowSolutions[platform];
			var result = solution.RootFolder.Build (monitor, configuration);
			if (Built != null)
			{
				Built(this, new SolutionItemEventArgs(null, this));
			}
			return result;
	    }

		public event SolutionItemEventHandler Built;

		public event SolutionItemEventHandler DefinitionBuilt;

		public event SolutionItemEventHandler Generated;

	    public void Generate (IProgressMonitor monitor, ConfigurationSelector configuration)
        {
            var platform = configuration.GetConfiguration(this).Id;
			EnsureProjectsAreGeneratedForPlatform(platform, monitor);
			if (Generated != null)
			{
				Generated(this, new SolutionItemEventArgs(null, this));
			}
	    }

        private object generateLock = new object ();

        private object generateRequestLock = new object ();

        private Dictionary<string, DateTime> generateRequestTime = new Dictionary<string, DateTime> ();

		public void ClearShadowSolutions ()
		{
			lock (generateLock)
			{
				shadowSolutions.Clear();
			}
		}

        private void EnsureProjectsAreGeneratedForPlatform(string platform, IProgressMonitor monitor)
        {
            var timestamp = DateTime.UtcNow;
            lock (generateRequestLock) {
                generateRequestTime[platform] = timestamp;
            }

            lock (generateLock) {
                if (!shadowSolutions.ContainsKey (platform)) {

                    lock (generateRequestLock) {
                        var generateAt = generateRequestTime.ContainsKey (platform)
                            ? generateRequestTime[platform]
                            : (DateTime?) null;
                        if (generateAt == null || timestamp < generateAt) {
                            // Request is not needed any more.
                            return;
                        }
                        generateRequestTime.Remove (platform);
                    }

                    try {
                        monitor.BeginTask ("Generating .NET projects for " + platform + "...", 1);
                        appDomain.RunExecutableWithArguments (latestModuleInfo, "--generate " + platform, monitor);
                    }
                    catch (Exception ex) {
                        monitor.ReportError ("Failed to generate projects", ex);
                        throw;
                    }
                    finally {
                        monitor.EndTask ();
                    }

                    try {
                        monitor.BeginTask ("Caching .NET solution for " + platform + "...", 1);
                        var path = Path.Combine (latestModuleInfo.Path,
                            latestModuleInfo.Name + "." + platform + ".sln");
                        var formats = Services.ProjectService.FileFormats.GetFileFormats (path, typeof (Solution));

                        if (formats.Length == 0)
                            throw new InvalidOperationException ("Unknown file format: " + path);

                        var format = formats[0];
                        object obj = format.Format.ReadFile (path, typeof (Solution), monitor);
                        if (obj == null)
                            throw new InvalidOperationException ("Invalid file format: " + path);

                        shadowSolutions[platform] = (Solution) obj;
                    }
                    catch (Exception ex) {
                        monitor.ReportError ("Failed to cache solution", ex);
                        throw;
                    }
                    finally {
                        monitor.EndTask ();
                    }
                }
            }
	    }

	    public void SetActiveConfiguration (string configuration)
	    {
	        if (ActiveConfiguration != configuration) {
                ActiveConfiguration = configuration;
                if (ShadowSolutionChanged != null)
                {
                    ShadowSolutionChanged(this, new SolutionItemEventArgs(null, this));
                }
	        }
	    }

	    public event SolutionItemEventHandler ShadowSolutionChanged;

	    public ProtobuildPackages Packages { get; set; }

        public ItemCollection<ProtobuildDefinition> Definitions { get; set; }

        public ItemCollection<ProtobuildSubmodule> Submodules { get; set; }

	    public Project GetShadowProject (ProtobuildDefinition protobuildDefinition, IProgressMonitor monitor,
	        ConfigurationSelector configuration)
	    {
            var platform = configuration.GetConfiguration(this).Id;
            EnsureProjectsAreGeneratedForPlatform(platform, monitor);

            var solution = shadowSolutions[platform];
            return (Project)solution.Items.OfType<Project>().FirstOrDefault(x => x.BaseDirectory == protobuildDefinition.ProjectDirectory);
	    }

	    public override ReadOnlyCollection<T> GetAllSolutionItems<T> ()
	    {
	        var collection = new List<T> ();

	        if (typeof (T).IsAssignableFrom (typeof (ProtobuildDefinition))) {
	            collection.AddRange (GetAllDefinitions().Cast<T> ());
            }

            if (typeof(T).IsAssignableFrom(typeof(ProtobuildPackage)))
            {
                collection.AddRange(GetAllPackages().Cast<T>());
            }

            if (typeof(T).IsAssignableFrom(typeof(ProtobuildSubmodule)))
            {
                collection.AddRange(GetAllSubmodules().Cast<T>());
            }

	        return collection.AsReadOnly ();
	    }

	    private IEnumerable<ProtobuildPackage> GetAllPackages ()
	    {
            throw new NotImplementedException();
	    }

	    private IEnumerable<ProtobuildDefinition> GetAllDefinitions ()
	    {
	        return WalkModuleTreeForDefinitions (this);
	    }

	    private static IEnumerable<ProtobuildDefinition> WalkModuleTreeForDefinitions (IProtobuildModule module)
	    {
            if (module.Definitions != null)
            {
                foreach (var definition in module.Definitions)
                {
                    yield return definition;
                }
            }

            if (module.Packages != null)
            {
                foreach (var package in module.Packages)
                {
                    foreach (var definition in WalkModuleTreeForDefinitions(package))
                    {
                        yield return definition;
                    }
                }
            }

            if (module.Submodules != null)
            {
                foreach (var submodule in module.Submodules)
                {
                    foreach (var definition in WalkModuleTreeForDefinitions(submodule))
                    {
                        yield return definition;
                    }
                }
            }
	    }

	    private IEnumerable<ProtobuildSubmodule> GetAllSubmodules ()
        {
            throw new NotImplementedException();
	    }

	    public Project GetShadowProject (ProtobuildDefinition protobuildDefinition, string platform)
        {
	        if (!shadowSolutions.ContainsKey (platform)) {
	            return null;
	        }
            var solution = shadowSolutions[platform];
            return (Project)solution.Items.OfType<Project>().FirstOrDefault(x => x.BaseDirectory == protobuildDefinition.ProjectDirectory);
        }

        public override ReadOnlyCollection<string> GetConfigurations ()
        {
            return new[] { "Debug" }.ToList().AsReadOnly();
        }
    }

    public interface IProtobuildModule
    {
        ProtobuildPackages Packages { get; set; }

        ItemCollection<ProtobuildDefinition> Definitions { get; set; }

        ItemCollection<ProtobuildSubmodule> Submodules { get; set; }

		SolutionFolder RootFolder { get; set; }

		string[] SupportedPlatformsArray { get; }
    }
}

