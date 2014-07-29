/**
 * Kopernicus Planetary System Modifier
 * Copyright (C) 2014 Bryce C Schroeder (bryce.schroeder@gmail.com), Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * http://www.ferazelhosting.net/~bryce/contact.html
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301  USA
 * 
 * This library is intended to be used as a plugin for Kerbal Space Program
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kopernicus
{
	// This class will add a planet. It will be supplanted by
	// versions which do more useful things, such as loading a planet from
	// configuration files, loading and modifying a stock planet from KSP's
	// resources (e.g. to allow modders to create modifications of the stock
	// Kerbol system, or to support backward compatibility with PlanetFactory),
	// or even procedurally generating a planet.
	// DOES NOT CURRENTLY HANDLE THE CASE OF GENERATING THE ROOT (Sun)
	// This code necessarily has side effects of execution, because of the way AddBody
	// works (we can't make a planet *not* in a system and return it.)
	public class KopernicusPlanetSource
	{
		public static PSystemBody GeneratePlanet (PSystem system, Orbit orbit = null) {
			return GenerateSystemBody (system, system.rootBody, orbit);
		}
		public static PSystemBody GenerateSystemBody(PSystem system, PSystemBody parent, Orbit orbit = null) 
		{
			// Use Dres as the template to clone ( :( )
			PSystemBody Dres = KopernicusUtility.FindBody (system.rootBody, "Dres");

			// AddBody makes the GameObject and stuff. It also attaches it to the system and parent.
			PSystemBody body = system.AddBody (parent);

			// set up the various parameters
			body.name = "Kopernicus";
			body.orbitRenderer.orbitColor = Color.magenta;
			body.flightGlobalsIndex = 100;
			body.pqsVersion = Dres.pqsVersion;

			// Some parameters of the celestialBody, which represents the actual planet...
			// PSystemBody is more of a container that associates the planet with its orbit 
			// and position in the planetary system, etc.
			body.celestialBody.bodyName = "Kopernicus";
			body.celestialBody.bodyDescription = "Merciful Kod, this thing just APPEARED! And unlike last time, it wasn't bird droppings on the telescope.";
			body.celestialBody.Radius = 300000;
			body.celestialBody.GeeASL = 0.33; // This is g, not acceleration due to g, it turns out.
			body.celestialBody.gravParameter = 398600.0; // guessing this is the Standard gravitational parameter, i.e. mu
			body.celestialBody.timeWarpAltitudeLimits = (float[])Dres.celestialBody.timeWarpAltitudeLimits.Clone();
			
			// Presumably true of Kerbin. I do not know what the consequences are of messing with this exactly.
			body.celestialBody.isHomeWorld = false;
			//body.celestialBody.gMagnitudeAtCenter = prototype.celestialBody.gMagnitudeAtCenter; // function unknown at this time

			// Setup the orbit of "Kopernicus."  The "Orbit" class actually is built to support serialization straight
			// from Squad, so storing these to files (and loading them) will be pretty easy.
			body.orbitDriver.celestialBody = body.celestialBody;
			body.orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;
			body.orbitDriver.UpdateOrbit ();
			if (orbit == null) 
				body.orbitDriver.orbit = new Orbit (0.0, 0.0, 150000000000, 0, 0, 0, 0, system.rootBody.celestialBody);
			else
				body.orbitDriver.orbit = orbit;


			#region PSystemBody.pqsVersion generation

			// Create the PQS controller for Kopernicus
			/*GameObject controllerRoot = new GameObject("Kopernicus");
			//controllerRoot.transform.parent = body.celestialBody.transform;
			body.pqsVersion = controllerRoot.AddComponent<PQS>();
			body.pqsVersion.surfaceMaterial = new Material(Shader.Find("Terrain/PQS/Sphere Projection SURFACE QUAD"));
			body.pqsVersion.fallbackMaterial = new Material(Shader.Find("Terrain/PQS/Sphere Projection SURFACE QUAD (Fallback) "));
			body.pqsVersion.radius = 300000;
			body.pqsVersion.meshRecieveShadows = true;

			// Create the celestial body transform
			GameObject mod = new GameObject("_CelestialBody");
			mod.transform.parent = controllerRoot.transform;
			PQSMod_CelestialBodyTransform celestialBodyTransform = mod.AddComponent<PQSMod_CelestialBodyTransform>();
			celestialBodyTransform.sphere = body.pqsVersion;
			celestialBodyTransform.body = body.celestialBody;
			celestialBodyTransform.forceActivate = false;
			celestialBodyTransform.deactivateAltitude = 115000;
			celestialBodyTransform.forceRebuildOnTargetChange = false;
			celestialBodyTransform.planetFade = new PQSMod_CelestialBodyTransform.AltitudeFade();
			celestialBodyTransform.planetFade.fadeFloatName = "_PlanetOpacity";
			celestialBodyTransform.planetFade.fadeStart = 100000.0f;
			celestialBodyTransform.planetFade.fadeEnd = 110000.0f;
			celestialBodyTransform.planetFade.valueStart = 0.0f;
			celestialBodyTransform.planetFade.valueEnd = 1.0f;
			celestialBodyTransform.planetFade.secondaryRenderers = new List<GameObject>();
			celestialBodyTransform.secondaryFades = new PQSMod_CelestialBodyTransform.AltitudeFade[0];
			celestialBodyTransform.requirements = PQS.ModiferRequirements.Default;
			celestialBodyTransform.modEnabled = true;
			celestialBodyTransform.order = 10;
			//body.pqsVersion.RebuildSphere();

			// Create the color PQS mods
			mod = new GameObject("_Color");
			mod.transform.parent = controllerRoot.transform;
			PQSMod_VertexSimplexNoiseColor vertexSimplexNoiseColor = mod.AddComponent<PQSMod_VertexSimplexNoiseColor>();
			vertexSimplexNoiseColor.sphere = body.pqsVersion;
			vertexSimplexNoiseColor.seed = 45;
			vertexSimplexNoiseColor.blend = 1.0f;
			vertexSimplexNoiseColor.colorStart = new Color(0.768656731f, 0.6996614f, 0.653089464f, 1);
			vertexSimplexNoiseColor.colorEnd = new Color(0.0f, 0.0f, 0.0f, 1.0f);
			vertexSimplexNoiseColor.octaves = 12.0;
			vertexSimplexNoiseColor.persistence = 0.5;
			vertexSimplexNoiseColor.frequency = 2.0;
			vertexSimplexNoiseColor.requirements = PQS.ModiferRequirements.MeshColorChannel;
			vertexSimplexNoiseColor.modEnabled = true;
			vertexSimplexNoiseColor.order = 200;
			//body.pqsVersion.RebuildSphere();

			PQSMod_HeightColorMap heightColorMap = mod.AddComponent<PQSMod_HeightColorMap>();
			heightColorMap.sphere = body.pqsVersion;
			List<PQSMod_HeightColorMap.LandClass> landClasses = new List<PQSMod_HeightColorMap.LandClass>();

			PQSMod_HeightColorMap.LandClass landClass = new PQSMod_HeightColorMap.LandClass("AbyPl", 0.0, 0.5, new Color(0.0f, 0.0f, 0.0f, 1.0f), Color.white, double.NaN);
			landClass.lerpToNext = true;
			landClasses.Add(landClass);

			landClass = new PQSMod_HeightColorMap.LandClass("Beach", 0.5, 0.550000011920929, new Color(0.164179087f, 0.164179087f, 0.164179087f, 1.0f), Color.white, double.NaN);
			landClass.lerpToNext = true;
			landClasses.Add(landClass);

			landClass = new PQSMod_HeightColorMap.LandClass("Beach", 0.550000011920929, 1.0, new Color(0.373134315f, 0.373134315f, 0.373134315f, 1.0f), Color.white, double.NaN);
			landClass.lerpToNext = false;
			landClasses.Add(landClass);

			// Generate an array from the land classes list
			heightColorMap.landClasses = landClasses.ToArray();
			heightColorMap.blend = 0.7f;
			heightColorMap.lcCount = 3;
			heightColorMap.requirements = PQS.ModiferRequirements.MeshColorChannel;
			heightColorMap.modEnabled = true;
			heightColorMap.order = 201;
			//body.pqsVersion.RebuildSphere();

			// Create the alititude alpha mods
			mod = new GameObject("_Material_ModProjection");
			mod.transform.parent = controllerRoot.transform;
			PQSMod_AltitudeAlpha altitudeAlpha = mod.AddComponent<PQSMod_AltitudeAlpha>();
			altitudeAlpha.sphere = body.pqsVersion;
			altitudeAlpha.atmosphereDepth = 4000.0;
			altitudeAlpha.invert = false;
			altitudeAlpha.requirements = PQS.ModiferRequirements.Default;
			altitudeAlpha.modEnabled = false;
			altitudeAlpha.order = 999999999;
			//body.pqsVersion.RebuildSphere();

			// Create the aerial perspective material
			mod = new GameObject("_Material_AerialPerspective");
			mod.transform.parent = controllerRoot.transform;
			PQSMod_AerialPerspectiveMaterial aerialPerspectiveMaterial = mod.AddComponent<PQSMod_AerialPerspectiveMaterial>();
			aerialPerspectiveMaterial.sphere = body.pqsVersion;
			aerialPerspectiveMaterial.globalDensity = -0.00001f;
			aerialPerspectiveMaterial.heightFalloff = 6.75f;
			aerialPerspectiveMaterial.atmosphereDepth = 150000;
			aerialPerspectiveMaterial.DEBUG_SetEveryFrame = true;
			aerialPerspectiveMaterial.cameraAlt = 0;
			aerialPerspectiveMaterial.cameraAtmosAlt = 0;
			aerialPerspectiveMaterial.heightDensAtViewer = 0;
			aerialPerspectiveMaterial.requirements = PQS.ModiferRequirements.Default;
			aerialPerspectiveMaterial.modEnabled = true;
			aerialPerspectiveMaterial.order = 100;
			//body.pqsVersion.RebuildSphere();

			// Create the UV planet relative position
			mod = new GameObject("_Material_SurfaceQuads");
			mod.transform.parent = controllerRoot.transform;
			PQSMod_UVPlanetRelativePosition planetRelativePosition = mod.AddComponent<PQSMod_UVPlanetRelativePosition>();
			planetRelativePosition.sphere = body.pqsVersion;
			planetRelativePosition.requirements = PQS.ModiferRequirements.Default;
			planetRelativePosition.modEnabled = true;
			planetRelativePosition.order = 999999;
			//body.pqsVersion.RebuildSphere();

			// Create the height noise module
			mod = new GameObject("_HeightNoise");
			mod.transform.parent = controllerRoot.transform;
			PQSMod_VertexHeightMap vertexHeightMap = mod.AddComponent<PQSMod_VertexHeightMap>();
			vertexHeightMap.sphere = body.pqsVersion;
			vertexHeightMap.heightMapDeformity = 3000.0;
			vertexHeightMap.heightMapOffset = 0.0;
			vertexHeightMap.scaleDeformityByRadius = false;
			vertexHeightMap.requirements = PQS.ModiferRequirements.MeshCustomNormals | PQS.ModiferRequirements.VertexMapCoords;
			vertexHeightMap.modEnabled = true;
			vertexHeightMap.order = 20;

			// Load the heightmap for this planet
			Texture2D map = new Texture2D(4, 4, TextureFormat.Alpha8, false);
			map.LoadImage(System.IO.File.ReadAllBytes(KSPUtil.ApplicationRootPath + "GameData/Kopernicus/Plugins/PluginData/CallistoHeight.png"));
			vertexHeightMap.heightMap = ScriptableObject.CreateInstance<MapSO>();
			vertexHeightMap.heightMap.CreateMap(MapSO.MapDepth.Greyscale, map);
			UnityEngine.Object.DestroyImmediate(map);
			//body.pqsVersion.RebuildSphere();

			// Create the simplex height module
			PQSMod_VertexSimplexHeight vertexSimplexHeight = mod.AddComponent<PQSMod_VertexSimplexHeight>();
			vertexSimplexHeight.sphere = body.pqsVersion;
			vertexSimplexHeight.seed = 670000;
			vertexSimplexHeight.deformity = 1700.0;
			vertexSimplexHeight.octaves = 12.0;
			vertexSimplexHeight.persistence = 0.5;
			vertexSimplexHeight.frequency = 4.0;
			vertexSimplexHeight.requirements = PQS.ModiferRequirements.MeshCustomNormals;
			vertexSimplexHeight.modEnabled = true;
			vertexSimplexHeight.order = 21;
			//body.pqsVersion.RebuildSphere();

			// Create the flatten ocean module
			PQSMod_FlattenOcean flattenOcean = mod.AddComponent<PQSMod_FlattenOcean>();
			flattenOcean.sphere = body.pqsVersion;
			flattenOcean.oceanRadius = 1.0;
			flattenOcean.requirements = PQS.ModiferRequirements.MeshCustomNormals;
			flattenOcean.modEnabled = true;
			flattenOcean.order = 25;
			//body.pqsVersion.RebuildSphere();

			// Creat the vertex height noise module
			PQSMod_VertexHeightNoise vertexHeightNoise = mod.AddComponent<PQSMod_VertexHeightNoise>();
			vertexHeightNoise.sphere = body.pqsVersion;
			vertexHeightNoise.noiseType = PQSMod_VertexHeightNoise.NoiseType.RiggedMultifractal;
			vertexHeightNoise.deformity = 1000.0f;
			vertexHeightNoise.seed = 5906;
			vertexHeightNoise.frequency = 2.0f;
			vertexHeightNoise.lacunarity = 2.5f;
			vertexHeightNoise.persistance = 0.5f;
			vertexHeightNoise.octaves = 4;
			vertexHeightNoise.mode = LibNoise.Unity.QualityMode.Low;
			vertexHeightNoise.requirements = PQS.ModiferRequirements.MeshColorChannel;
			vertexHeightNoise.modEnabled = true;
			vertexHeightNoise.order = 22;
			//body.pqsVersion.RebuildSphere();

			// Create the material direction
			mod = new GameObject("_Material_SunLight");
			mod.transform.parent = controllerRoot.gameObject.transform;
			PQSMod_MaterialSetDirection materialSetDirection = mod.AddComponent<PQSMod_MaterialSetDirection>();
			materialSetDirection.sphere = body.pqsVersion;
			materialSetDirection.valueName = "_sunLightDirection";
			materialSetDirection.requirements = PQS.ModiferRequirements.Default;
			materialSetDirection.modEnabled = true;
			materialSetDirection.order = 100;
			//body.pqsVersion.RebuildSphere();

			// Crete the quad mesh colliders
			mod = new GameObject("QuadMeshColliders");
			mod.transform.parent = controllerRoot.gameObject.transform;
			PQSMod_QuadMeshColliders quadMeshColliders = mod.AddComponent<PQSMod_QuadMeshColliders>();
			quadMeshColliders.sphere = body.pqsVersion;
			quadMeshColliders.maxLevelOffset = 0;
			quadMeshColliders.physicsMaterial = new PhysicMaterial();
			quadMeshColliders.physicsMaterial.name = "Ground";
			quadMeshColliders.physicsMaterial.dynamicFriction = 0.6f;
			quadMeshColliders.physicsMaterial.staticFriction = 0.8f;
			quadMeshColliders.physicsMaterial.bounciness = 0.0f;
			quadMeshColliders.physicsMaterial.frictionDirection2 = Vector3.zero;
			quadMeshColliders.physicsMaterial.dynamicFriction2 = 0.0f;
			quadMeshColliders.physicsMaterial.staticFriction2 = 0.0f;
			quadMeshColliders.physicsMaterial.frictionCombine = PhysicMaterialCombine.Maximum;
			quadMeshColliders.physicsMaterial.bounceCombine = PhysicMaterialCombine.Average;
			quadMeshColliders.requirements = PQS.ModiferRequirements.Default;
			quadMeshColliders.modEnabled = true;
			quadMeshColliders.order = 100;
			//body.pqsVersion.RebuildSphere();

			// Create the simplex height absolute
			mod = new GameObject("_FineDetail");
			mod.transform.parent = controllerRoot.gameObject.transform;
			PQSMod_VertexSimplexHeightAbsolute vertexSimplexHeightAbsolute = mod.AddComponent<PQSMod_VertexSimplexHeightAbsolute>();
			vertexSimplexHeightAbsolute.sphere = body.pqsVersion;
			vertexSimplexHeightAbsolute.seed = 4234;
			vertexSimplexHeightAbsolute.deformity = 400.0;
			vertexSimplexHeightAbsolute.octaves = 6.0;
			vertexSimplexHeightAbsolute.persistence = 0.5;
			vertexSimplexHeightAbsolute.frequency = 18.0;
			vertexSimplexHeightAbsolute.requirements = PQS.ModiferRequirements.Default;
			vertexSimplexHeightAbsolute.modEnabled = true;
			vertexSimplexHeightAbsolute.order = 30;
			//body.pqsVersion.RebuildSphere();

			// Surface color map
			mod = new GameObject("_LandClass");
			mod.transform.parent = controllerRoot.gameObject.transform;
			PQSMod_VertexColorMapBlend colorMap = mod.AddComponent<PQSMod_VertexColorMapBlend>();
			colorMap.sphere = body.pqsVersion;
			colorMap.blend = 1.0f;
			colorMap.order = 500;
			colorMap.modEnabled = true;
			//body.pqsVersion.RebuildSphere();

			// Decompress and load the color
			map = new Texture2D(4, 4, TextureFormat.RGB24, false);
			map.LoadImage(System.IO.File.ReadAllBytes(KSPUtil.ApplicationRootPath + "GameData/Kopernicus/Plugins/PluginData/CallistoColor.png"));
			colorMap.vertexColorMap = ScriptableObject.CreateInstance<MapSO>();
			colorMap.vertexColorMap.CreateMap(MapSO.MapDepth.RGB, map);
			UnityEngine.Object.DestroyImmediate(map);
			//body.pqsVersion.RebuildSphere();*/

			#endregion

			#region PSystemBody.scaledVersion generation
			// Create the scaled version of the planet for use in map view (i've tried generating it on my own but it just doesn't appear.  hmm)
			body.scaledVersion = (GameObject) UnityEngine.Object.Instantiate(Dres.scaledVersion);
			body.scaledVersion.name = "Kopernicus";
			//body.scaledVersion = new GameObject("Kopernicus");

			// Scale the mesh to the new body size
			MeshFilter meshFilter = body.scaledVersion.GetComponent<MeshFilter> ();
			Mesh scaledMesh = new Mesh();
			KopernicusUtility.CopyMesh(Dres.scaledVersion.GetComponent<MeshFilter>().sharedMesh, scaledMesh);
			meshFilter.mesh = scaledMesh;

			// Load and compress the color texture for the custom planet
			Texture2D colorTexture = new Texture2D(4, 4, TextureFormat.RGBA32, true);
			colorTexture.LoadImage(System.IO.File.ReadAllBytes(KSPUtil.ApplicationRootPath + "GameData/Kopernicus/Plugins/PluginData/CallistoColor.png"));
			colorTexture.Compress(true);
			colorTexture.Apply(true, true);

			// Load and compress the color texture for the custom planet
			Texture2D bumpTexture = new Texture2D(4, 4, TextureFormat.RGBA32, true);
			bumpTexture.LoadImage(System.IO.File.ReadAllBytes(KSPUtil.ApplicationRootPath + "GameData/Kopernicus/Plugins/PluginData/Callisto_NRM.png"));
			bumpTexture.Compress(true);
			bumpTexture.Apply(true, true);

			// Write a new material for this texture
			body.scaledVersion.renderer.material = new Material(Dres.scaledVersion.renderer.material.shader);
			body.scaledVersion.renderer.material.SetTexture("_MainTex", colorTexture);
			body.scaledVersion.renderer.material.SetTexture("_BumpMap", bumpTexture);

			// Create the sphere collider
			/*SphereCollider collider = body.scaledVersion.AddComponent<SphereCollider> ();
			collider.center         = Vector3.zero;
			collider.radius         = 1000.0f;*/

			// Create the ScaledSpaceFader to fade the orbit out where we view it (maybe?)
			//ScaledSpaceFader fader = body.scaledVersion.AddComponent<ScaledSpaceFader> ();
			ScaledSpaceFader fader = body.scaledVersion.GetComponent<ScaledSpaceFader> ();
			fader.celestialBody    = body.celestialBody;
			fader.fadeStart        = 95000.0f;
			fader.fadeEnd          = 100000.0f;
			fader.floatName        = "_Opacity";
		
#endregion
			// Print out a walk of the body
			//KopernicusUtility.GameObjectWalk (body.pqsVersion.gameObject);;

			// Return the new body
			return body;
		}


	}

	// Add a clone of a stock planet (in a different orbit)
	// This is a kind of KopernicusPlanetSource
	public class StockPlanetSource : KopernicusPlanetSource {
		public static PSystemBody GeneratePlanet (PSystem system, string stockPlanetName, string newName, Orbit orbit = null) {
			return GenerateSystemBody (system, system.rootBody, stockPlanetName, newName, orbit);
		}

		public static PSystemBody GenerateSystemBody(PSystem system, PSystemBody parent, string stockPlanetName, string newName, Orbit orbit = null) {
			// AddBody makes the GameObject and stuff. It also attaches it to the system and
			// parent.
			PSystemBody body = system.AddBody (parent);
			PSystemBody prototype = KopernicusUtility.FindBody (system.rootBody, stockPlanetName);

			if (prototype == null) {
				Debug.Log ("Kopernicus:StockPlanetSource can't find a stock planet named " + stockPlanetName);
				return null;
			}

			// set up the various parameters
			body.name = newName;
			body.orbitRenderer.orbitColor = prototype.orbitRenderer.orbitColor;
			body.flightGlobalsIndex = prototype.flightGlobalsIndex;

			// Some parameters of the celestialBody, which represents the actual planet...
			// PSystemBody is more of a container that associates the planet with its orbit 
			// and position in the planetary system, etc.
			body.celestialBody.bodyName = newName;
			body.celestialBody.Radius = prototype.celestialBody.Radius;

			// This is g, not acceleration due to g, it turns out.
			body.celestialBody.GeeASL = prototype.celestialBody.GeeASL; 
			// This is the Standard gravitational parameter, i.e. mu
			body.celestialBody.gravParameter = prototype.celestialBody.gravParameter; 

			// It appears that it calculates SOI for you if you give it this stuff.
			body.celestialBody.bodyDescription = prototype.celestialBody.bodyDescription;
			// at the moment, this value is always "Generic" but I guess that might change.
			body.celestialBody.bodyType = prototype.celestialBody.bodyType;

			// Presumably true of Kerbin. I do not know what the consequences are of messing with this exactly.
			body.celestialBody.isHomeWorld = prototype.celestialBody.isHomeWorld;
			// function unknown at this time
			body.celestialBody.gMagnitudeAtCenter = prototype.celestialBody.gMagnitudeAtCenter;
			// time warp limits
			body.celestialBody.timeWarpAltitudeLimits = (float[])prototype.celestialBody.timeWarpAltitudeLimits.Clone();

			// Setup the orbit of "Kopernicus."  The "Orbit" class actually is built to support serialization straight
			// from Squad, so storing these to files (and loading them) will be pretty easy.

			//Debug.Log ("..About to assign orbit.");

			// Note that we may have to adjust the celestialBody target here, because odds are
			// we're putting a planet found in the systemPrefab into a system that is not the
			// systemPrefab and has different celestialbodies. FIXME by having it look up the 
			// body by name perhaps? That will break of course if you have, say, two Jools.

			if (orbit == null)
				body.orbitDriver.orbit = prototype.orbitDriver.orbit; // probably won't work if not going into a cloned systemprefab Sun
			else
				body.orbitDriver.orbit = orbit;

 

			body.orbitDriver.celestialBody = body.celestialBody;
			body.orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;
			body.orbitDriver.UpdateOrbit ();

			//Debug.Log ("..About to clone the scaledversion.");
			// Temporarily clone the Dres scaled version for the structure
			// Find the dres prefab
			GameObject scaledVersion = (GameObject)UnityEngine.Object.Instantiate(prototype.scaledVersion);
			/*if (scaledVersion == null)
				Debug.Log ("ScaledVersion is null");
			else
				Debug.Log ("ScaledVersion is not null.");*/
			body.scaledVersion = scaledVersion;

			//Debug.Log ("..About to assign fader.");
			// Adjust the scaled space fader to our new celestial body
			ScaledSpaceFader fader = scaledVersion.GetComponent<ScaledSpaceFader> ();
			//Debug.Log ("fader: " + fader + " sv:", scaledVersion);
			fader.celestialBody = body.celestialBody;

			//Debug.Log ("..done.");
			return body;

		}
	}
}

