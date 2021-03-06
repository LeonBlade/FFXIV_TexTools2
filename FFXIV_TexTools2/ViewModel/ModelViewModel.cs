﻿// FFXIV TexTools
// Copyright © 2017 Rafael Gonzalez - All Rights Reserved
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using FFXIV_TexTools2.Helpers;
using FFXIV_TexTools2.IO;
using FFXIV_TexTools2.Material;
using FFXIV_TexTools2.Model;
using FFXIV_TexTools2.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace FFXIV_TexTools2.ViewModel
{
    public class ModelViewModel : INotifyPropertyChanged
    {
        TEXData texInfo;
        MTRLData mtrlInfo;
        ItemData selectedItem;
        List<ModelMeshData> meshList;
        List<string> materialStrings;
        List<MDLTEXData> meshData;
        Composite3DViewModel CVM = new Composite3DViewModel();

        int raceIndex, meshIndex, bodyIndex, partIndex;
        bool raceEnabled, meshEnabled, bodyEnabled, partEnabled, modelRendering, secondModelRendering, thirdModelRendering, is3DLoaded, disposing, modelTabEnabled;
        string selectedCategory, reflectionAmount, modelName;

        private ObservableCollection<ComboBoxInfo> raceComboInfo = new ObservableCollection<ComboBoxInfo>();
        private ObservableCollection<ComboBoxInfo> meshComboInfo = new ObservableCollection<ComboBoxInfo>();
        private ObservableCollection<ComboBoxInfo> partComboInfo = new ObservableCollection<ComboBoxInfo>();
        private ObservableCollection<ComboBoxInfo> bodyComboInfo = new ObservableCollection<ComboBoxInfo>();

        private ComboBoxInfo selectedRace;
        private ComboBoxInfo selectedMesh;
        private ComboBoxInfo selectedPart;
        private ComboBoxInfo selectedBody;

        public int RaceIndex { get { return raceIndex; } set { raceIndex = value; NotifyPropertyChanged("RaceIndex"); } }
        public int MeshIndex { get { return meshIndex; } set { meshIndex = value; NotifyPropertyChanged("MeshIndex"); } }
        public int BodyIndex { get { return bodyIndex; } set { bodyIndex = value; NotifyPropertyChanged("BodyIndex"); } }
        public int PartIndex { get { return partIndex; } set { partIndex = value; NotifyPropertyChanged("PartIndex"); } }

        public string ReflectionAmount { get { return reflectionAmount; } set { reflectionAmount = value; NotifyPropertyChanged("ReflectionAmount"); } }

        public bool RaceEnabled { get { return raceEnabled; } set { raceEnabled = value; NotifyPropertyChanged("RaceEnabled"); } }
        public bool MeshEnabled { get { return meshEnabled; } set { meshEnabled = value; NotifyPropertyChanged("MeshEnabled"); } }
        public bool BodyEnabled { get { return bodyEnabled; } set { bodyEnabled = value; NotifyPropertyChanged("BodyEnabled"); } }
        public bool PartEnabled { get { return partEnabled; } set { partEnabled = value; NotifyPropertyChanged("PartEnabled"); } }

        public bool ModelTabEnabled { get { return modelTabEnabled; } set { modelTabEnabled = value; NotifyPropertyChanged("ModelTabEnabled"); } }

        public bool ModelRendering { get { return modelRendering; } set { modelRendering = value; NotifyPropertyChanged("ModelRendering"); } }
        public bool SecondModelRendering { get { return secondModelRendering; } set { secondModelRendering = value; NotifyPropertyChanged("SecondModelRendering"); } }
        public bool ThirdModelRendering { get { return thirdModelRendering; } set { thirdModelRendering = value; NotifyPropertyChanged("ThirdModelRendering"); } }

        public Composite3DViewModel CompositeVM { get { return CVM; } set { CVM = value; NotifyPropertyChanged("CompositeVM"); } }

        /// <summary>
        /// View Model for model view.
        /// </summary>
        /// <param name="item">the currently selcted item.</param>
        /// <param name="category">The category of the item.</param>
        public ModelViewModel(ItemData item, string category)
        {
            selectedItem = item;
            selectedCategory = category;

            try
            {
                string categoryType = Helper.GetCategoryType(selectedCategory);
                List<ComboBoxInfo> cbi = new List<ComboBoxInfo>();
                string MDLFolder = "";
                string MDLFile = "";

                if (categoryType.Equals("weapon") || categoryType.Equals("food"))
                {
                    MDLFolder = "";
                    cbi.Add(new ComboBoxInfo() { Name = Strings.All, ID = Strings.All, IsNum = false });
                }
                else if (categoryType.Equals("accessory"))
                {
                    MDLFolder = string.Format(Strings.AccMDLFolder, selectedItem.PrimaryModelID);
                    MDLFile = string.Format(Strings.AccMDLFile, "{0}", selectedItem.PrimaryModelID, Info.slotAbr[selectedCategory]);
                }
                else if (categoryType.Equals("character"))
                {
                    if (selectedItem.ItemName.Equals(Strings.Body))
                    {
                        MDLFolder = Strings.BodyMDLFolder;
                    }
                    else if (selectedItem.ItemName.Equals(Strings.Face))
                    {
                        MDLFolder = Strings.FaceMDLFolder;
                    }
                    else if (selectedItem.ItemName.Equals(Strings.Hair))
                    {
                        MDLFolder = Strings.HairMDLFolder;
                    }
                    else if (selectedItem.ItemName.Equals(Strings.Tail))
                    {
                        MDLFolder = Strings.TailMDLFolder;
                    }
                }
                else if (categoryType.Equals("monster"))
                {
                    cbi.Add(new ComboBoxInfo() { Name = Strings.All, ID = Strings.All, IsNum = false });
                }
                else
                {
                    MDLFolder = string.Format(Strings.EquipMDLFolder, selectedItem.PrimaryModelID);
                    MDLFile = string.Format(Strings.EquipMDLFile, "{0}", selectedItem.PrimaryModelID, Info.slotAbr[selectedCategory]);
                }

                var fileHashList = Helper.GetAllFilesInFolder(FFCRC.GetHash(MDLFolder));

                if (!categoryType.Equals("weapon") && !categoryType.Equals("monster"))
                {
                    foreach (string raceID in Info.IDRace.Keys)
                    {
                        if (categoryType.Equals("character"))
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                var mdlFolder = String.Format(MDLFolder, raceID, i.ToString().PadLeft(4, '0'));

                                if (Helper.FolderExists(FFCRC.GetHash(mdlFolder)))
                                {
                                    cbi.Add(new ComboBoxInfo() { Name = Info.IDRace[raceID], ID = raceID, IsNum = false });
                                    break;
                                }
                            }
                        }
                        else
                        {
                            var mdlFile = String.Format(MDLFile, raceID);
                            var fileHash = FFCRC.GetHash(mdlFile);

                            if (fileHashList.Contains(fileHash))
                            {
                                cbi.Add(new ComboBoxInfo() { Name = Info.IDRace[raceID], ID = raceID, IsNum = false });
                            }
                        }
                    }
                }

                RaceComboBox = new ObservableCollection<ComboBoxInfo>(cbi);
                RaceIndex = 0;

                if (cbi.Count <= 1)
                {
                    RaceEnabled = false;
                }
                else
                {
                    RaceEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[Main] tab 3D Error \n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Command for the Transparency button
        /// </summary>
        public ICommand TransparencyCommand
        {
            get { return new RelayCommand(SetTransparency); }
        }

        /// <summary>
        /// Sets the transparency of the model
        /// </summary>
        /// <param name="obj"></param>
        private void SetTransparency(object obj)
        {
            CompositeVM.Transparency();
        }

        /// <summary>
        /// Command for the Reflection button
        /// </summary>
        public ICommand ReflectionCommand
        {
            get { return new RelayCommand(SetReflection); }
        }


        /// <summary>
        /// Sets the reflectivity(Specular Shininess) of the model
        /// </summary>
        /// <param name="obj"></param>
        private void SetReflection(object obj)
        {
            CompositeVM.Reflections(selectedItem.ItemName);

            if (selectedItem.ItemName.Equals(Strings.Face) || selectedItem.ItemName.Equals(Strings.Face))
            {
                ReflectionAmount = String.Format("{0:0.##}", CompositeVM.ModelMaterial.SpecularShininess);
            }
            else
            {
                ReflectionAmount = String.Format("{0:0.##}", CompositeVM.SecondModelMaterial.SpecularShininess);
            }
        }

        /// <summary>
        /// Command for the Lighting button
        /// </summary>
        public ICommand LightingCommand
        {
            get { return new RelayCommand(SetLighting); }
        }

        /// <summary>
        /// Sets the lighting of the scene
        /// </summary>
        /// <param name="obj"></param>
        private void SetLighting(object obj)
        {
            CompositeVM.Lighting();
        }

        /// <summary>
        /// Command for the Export OBJ button
        /// </summary>
        public ICommand ExportOBJCommand
        {
            get { return new RelayCommand(ExportOBJ); }
        }

        /// <summary>
        /// Saves the model and created textures as an OBJ file
        /// </summary>
        /// <param name="obj"></param>
        private void ExportOBJ(object obj)
        {
            SaveModel.Save(selectedCategory, modelName, SelectedMesh.ID, selectedItem.ItemName, meshData, meshList);
        }
        
        /// <summary>
        /// Disposes of the view model data
        /// </summary>
        public void Dispose()
        {
            if (CompositeVM != null)
            {
                CompositeVM.Dispose();
            }
        }

        /// <summary>
        /// Item source for combo box containing available races for the model
        /// </summary>
        public ObservableCollection<ComboBoxInfo> RaceComboBox
        {
            get { return raceComboInfo; }
            set
            {
                if (raceComboInfo != null)
                {
                    raceComboInfo = value;
                    NotifyPropertyChanged("RaceComboBox");
                }
            }
        }
        
        /// <summary>
        /// Selected item for race combo box
        /// </summary>
        public ComboBoxInfo SelectedRace
        {
            get { return selectedRace; }
            set
            {
                if (value.Name != null)
                {
                    selectedRace = value;
                    NotifyPropertyChanged("SelectedRace");
                    RaceComboBoxChanged();
                }
            }
        }

        /// <summary>
        /// Sets the data for the body combo box
        /// </summary>
        private void RaceComboBoxChanged()
        {
            is3DLoaded = false;

            if (CompositeVM != null && !disposing)
            {
                disposing = true;
                CompositeVM.Dispose();
            }

            List<ComboBoxInfo> cbi = new List<ComboBoxInfo>();
            string categoryType = Helper.GetCategoryType(selectedCategory);
            string MDLFolder = "";

            if (categoryType.Equals("weapon"))
            {
                cbi.Add(new ComboBoxInfo() { Name = selectedItem.PrimaryModelBody, ID = selectedItem.PrimaryModelBody, IsNum = false });
            }
            else if (categoryType.Equals("food"))
            {
                cbi.Add(new ComboBoxInfo() { Name = selectedItem.PrimaryModelBody, ID = selectedItem.PrimaryModelBody, IsNum = false });
            }
            else if (categoryType.Equals("accessory"))
            {
                cbi.Add(new ComboBoxInfo() { Name = "-", ID = "-", IsNum = false });
            }
            else if (categoryType.Equals("character"))
            {
                if (selectedItem.ItemName.Equals(Strings.Body))
                {
                    MDLFolder = string.Format(Strings.BodyMDLFolder, SelectedRace.ID, "{0}");
                }
                else if (selectedItem.ItemName.Equals(Strings.Face))
                {
                    MDLFolder = string.Format(Strings.FaceMDLFolder, SelectedRace.ID, "{0}");
                }
                else if (selectedItem.ItemName.Equals(Strings.Hair))
                {
                    MDLFolder = string.Format(Strings.HairMDLFolder, SelectedRace.ID, "{0}");
                }
                else if (selectedItem.ItemName.Equals(Strings.Tail))
                {
                    MDLFolder = string.Format(Strings.TailMDLFolder, SelectedRace.ID, "{0}");
                }
            }
            else if (categoryType.Equals("monster"))
            {
                cbi.Add(new ComboBoxInfo() { Name = selectedItem.PrimaryModelBody, ID = selectedItem.PrimaryModelBody, IsNum = false });
            }
            else
            {
                cbi.Add(new ComboBoxInfo() { Name = "-", ID = "-", IsNum = false });
            }


            if (categoryType.Equals("character"))
            {
                for (int i = 0; i < 50; i++)
                {
                    string folder = String.Format(MDLFolder, i.ToString().PadLeft(4, '0'));

                    if (Helper.FolderExists(FFCRC.GetHash(folder)))
                    {
                        cbi.Add(new ComboBoxInfo() { Name = i.ToString(), ID = i.ToString(), IsNum = true });

                        if (selectedItem.ItemName.Equals(Strings.Body))
                        {
                            break;
                        }
                    }
                }
            }

            BodyComboBox = new ObservableCollection<ComboBoxInfo>(cbi);
            BodyIndex = 0;

            if (cbi.Count <= 1)
            {
                BodyEnabled = false;
            }
            else
            {
                BodyEnabled = true;
            }
        }

        /// <summary>
        /// Item source for the body combo box
        /// </summary>
        public ObservableCollection<ComboBoxInfo> BodyComboBox
        {
            get { return bodyComboInfo; }
            set { bodyComboInfo = value; NotifyPropertyChanged("BodyComboBox"); }
        }

        /// <summary>
        /// Selected item for the body combo box
        /// </summary>
        public ComboBoxInfo SelectedBody
        {
            get { return selectedBody; }
            set
            {
                if (value != null)
                {
                    selectedBody = value;
                    NotifyPropertyChanged("SelectedBody");
                    BodyComboBoxChanged();
                }
            }
        }

        /// <summary>
        /// Sets the data for the part combo box
        /// </summary>
        private void BodyComboBoxChanged()
        {
            is3DLoaded = false;
            bool isDemiHuman = false;

            if (CompositeVM != null && !disposing)
            {
                disposing = true;
                CompositeVM.Dispose();
            }

            if (selectedItem.PrimaryMTRLFolder != null && selectedItem.PrimaryMTRLFolder.Contains("demihuman"))
            {
                isDemiHuman = true;
            }

            List<ComboBoxInfo> cbi = new List<ComboBoxInfo>();
            string type = Helper.GetCategoryType(selectedCategory);

            string MDLFolder = "";
            string MDLFile = "";
            string[] abrParts = null;

            if (type.Equals("character"))
            {
                if (selectedItem.ItemName.Equals(Strings.Body))
                {
                    MDLFolder = string.Format(Strings.BodyMDLFolder, SelectedRace.ID, SelectedBody.ID.PadLeft(4, '0'));
                    MDLFile = string.Format(Strings.BodyMDLFile, SelectedRace.ID, SelectedBody.ID.PadLeft(4, '0'), "{0}");

                    abrParts = new string[5] { "met", "glv", "dwn", "sho", "top" };
                }
                else if (selectedItem.ItemName.Equals(Strings.Face))
                {
                    MDLFolder = string.Format(Strings.FaceMDLFolder, SelectedRace.ID, SelectedBody.ID.PadLeft(4, '0'));
                    MDLFile = string.Format(Strings.FaceMDLFile, SelectedRace.ID, SelectedBody.ID.PadLeft(4, '0'), "{0}");

                    abrParts = new string[3] { "fac", "iri", "etc" };
                }
                else if (selectedItem.ItemName.Equals(Strings.Hair))
                {
                    MDLFolder = string.Format(Strings.HairMDLFolder, SelectedRace.ID, SelectedBody.ID.PadLeft(4, '0'));
                    MDLFile = string.Format(Strings.HairMDLFile, SelectedRace.ID, SelectedBody.ID.PadLeft(4, '0'), "{0}");

                    abrParts = new string[2] { "hir", "acc" };
                }
                else if (selectedItem.ItemName.Equals(Strings.Tail))
                {
                    MDLFolder = string.Format(Strings.TailMDLFolder, SelectedRace.ID, SelectedBody.ID.PadLeft(4, '0'));
                    MDLFile = string.Format(Strings.TailMDLFile, SelectedRace.ID, SelectedBody.ID.PadLeft(4, '0'), "{0}");

                    abrParts = new string[1] { "til" };
                }

                var fileHashList = Helper.GetAllFilesInFolder(FFCRC.GetHash(MDLFolder));

                foreach (string abrPart in abrParts)
                {
                    var file = String.Format(MDLFile, abrPart);

                    if (fileHashList.Contains(FFCRC.GetHash(file)))
                    {
                        if (selectedItem.ItemName.Equals(Strings.Body))
                        {
                            cbi.Add(new ComboBoxInfo() { Name = Info.slotAbr.FirstOrDefault(x => x.Value == abrPart).Key, ID = abrPart, IsNum = false });
                        }
                        else if (selectedItem.ItemName.Equals(Strings.Face))
                        {
                            cbi.Add(new ComboBoxInfo() { Name = Info.FaceTypes.FirstOrDefault(x => x.Value == abrPart).Key, ID = abrPart, IsNum = false });
                        }
                        else if (selectedItem.ItemName.Equals(Strings.Hair))
                        {
                            cbi.Add(new ComboBoxInfo() { Name = Info.HairTypes.FirstOrDefault(x => x.Value == abrPart).Key, ID = abrPart, IsNum = false });
                        }
                        else if (selectedItem.ItemName.Equals(Strings.Tail))
                        {
                            cbi.Add(new ComboBoxInfo() { Name = Strings.Tail, ID = abrPart, IsNum = false });
                        }
                    }
                }
            }
            else if(isDemiHuman)
            {
                MDLFolder = string.Format(Strings.DemiMDLFolder, selectedItem.PrimaryModelID, selectedItem.PrimaryModelBody);
                MDLFile = string.Format(Strings.DemiMDLFile, selectedItem.PrimaryModelID, selectedItem.PrimaryModelBody, "{0}");

                abrParts = new string[5] { "met", "glv", "dwn", "sho", "top" };

                var fileHashList = Helper.GetAllFilesInFolder(FFCRC.GetHash(MDLFolder));

                foreach (string abrPart in abrParts)
                {
                    var file = String.Format(MDLFile, abrPart);

                    if (fileHashList.Contains(FFCRC.GetHash(file)))
                    {
                        cbi.Add(new ComboBoxInfo() { Name = Info.slotAbr.FirstOrDefault(x => x.Value == abrPart).Key, ID = abrPart, IsNum = false });
                    }
                }
            }
            else if (type.Equals("weapon"))
            {
                if(selectedItem.SecondaryModelID != null)
                {
                    cbi.Add(new ComboBoxInfo() { Name = "Primary", ID = "Primary", IsNum = false });
                    cbi.Add(new ComboBoxInfo() { Name = "Secondary", ID = "Secondary", IsNum = false });

                }
                else
                {
                    cbi.Add(new ComboBoxInfo() { Name = "Primary", ID = "Primary", IsNum = false });

                }
            }
            else if(type.Equals("monster"))
            {
                cbi.Add(new ComboBoxInfo() { Name = "1", ID = "1", IsNum = false });
            }
            else
            {
                cbi.Add(new ComboBoxInfo() { Name = "-", ID = "-", IsNum = false });
            }

            PartComboBox = new ObservableCollection<ComboBoxInfo>(cbi);
            PartIndex = 0;

            if (cbi.Count <= 1)
            {
                PartEnabled = false;
            }
            else
            {
                PartEnabled = true;
            }
        }

        /// <summary>
        /// Item source of the part combo box
        /// </summary>
        public ObservableCollection<ComboBoxInfo> PartComboBox
        {
            get { return partComboInfo; }
            set { partComboInfo = value; NotifyPropertyChanged("PartComboBox"); }
        }

        /// <summary>
        /// selected item of the part combo box
        /// </summary>
        public ComboBoxInfo SelectedPart
        {
            get { return selectedPart; }
            set
            {
                if (value != null)
                {
                    selectedPart = value;
                    NotifyPropertyChanged("SelectedPart");
                    PartComboBoxChanged();
                }
            }
        }

        /// <summary>
        /// Sets the data for the mesh combo box
        /// </summary>
        private void PartComboBoxChanged()
        {
            is3DLoaded = false;

            if (CompositeVM != null && !disposing)
            {
                disposing = true;
                CompositeVM.Dispose();
            }


            try
            {
                List<ComboBoxInfo> cbi = new List<ComboBoxInfo>();

                MDL mdl = new MDL(selectedItem, selectedCategory, Info.raceID[SelectedRace.Name], SelectedBody.ID, SelectedPart.ID);
                meshList = mdl.GetMeshList();
                modelName = mdl.GetModelName();
                materialStrings = mdl.GetMaterialStrings();

                cbi.Add(new ComboBoxInfo() { Name = Strings.All, ID = Strings.All, IsNum = false });

                if (meshList.Count > 1)
                {
                    for (int i = 0; i < meshList.Count; i++)
                    {
                        cbi.Add(new ComboBoxInfo() { Name = i.ToString(), ID = i.ToString(), IsNum = true });
                    }
                }

                MeshComboBox = new ObservableCollection<ComboBoxInfo>(cbi);
                MeshIndex = 0;

                if (cbi.Count > 1)
                {
                    MeshEnabled = true;
                }
                else
                {
                    MeshEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[Main] part 3D Error \n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Item source for the mesh combo box
        /// </summary>
        public ObservableCollection<ComboBoxInfo> MeshComboBox
        {
            get { return meshComboInfo; }
            set { meshComboInfo = value; NotifyPropertyChanged("MeshComboBox"); }
        }

        /// <summary>
        /// Selected item for the mesh combo box
        /// </summary>
        public ComboBoxInfo SelectedMesh
        {
            get { return selectedMesh; }
            set
            {
                if (value != null)
                {
                    selectedMesh = value;
                    NotifyPropertyChanged("SelectedMesh");
                    MeshComboBoxChanged();
                }
            }
        }

        /// <summary>
        /// Gets the model data and sets the display viewmodel
        /// </summary>
        private void MeshComboBoxChanged()
        {
            if (!is3DLoaded)
            {
                disposing = false;

                meshData = new List<MDLTEXData>();

                for (int i = 0; i < meshList.Count; i++)
                {
                    BitmapSource specularBMP = null;
                    BitmapSource diffuseBMP = null;
                    BitmapSource normalBMP = null;
                    BitmapSource colorBMP = null;
                    BitmapSource alphaBMP = null;
                    BitmapSource maskBMP = null;

                    TEXData specularData = null;
                    TEXData diffuseData = null;
                    TEXData normalData = null;
                    TEXData maskData = null;

                    bool isBody = false;
                    bool isFace = false;

                    MTRLData mtrlData = MTRL3D(i);

                    if (selectedCategory.Equals(Strings.Character))
                    {
                        if (selectedItem.ItemName.Equals(Strings.Tail) || selectedItem.ItemName.Equals(Strings.Hair))
                        {
                            normalData = TEX.GetTex(mtrlData.NormalOffset);
                            specularData = TEX.GetTex(mtrlData.SpecularOffset);

                            if (mtrlData.DiffusePath != null)
                            {
                                diffuseData = TEX.GetTex(mtrlData.DiffuseOffset);
                            }

                            var maps = TexHelper.MakeCharacterMaps(normalData, diffuseData, null, specularData);

                            diffuseBMP = maps[0];
                            specularBMP = maps[1];
                            normalBMP = maps[2];
                            alphaBMP = maps[3];

                            specularData.Dispose();
                            normalData.Dispose();
                            if (diffuseData != null)
                            {
                                diffuseData.Dispose();
                            }
                        }

                        if (selectedItem.ItemName.Equals(Strings.Body))
                        {
                            normalData = TEX.GetTex(mtrlData.NormalOffset);
                            //specularTI = TEX.GetTex(mInfo.SpecularOffset);
                            diffuseData = TEX.GetTex(mtrlData.DiffuseOffset);

                            isBody = true;
                            diffuseBMP = Imaging.CreateBitmapSourceFromHBitmap(diffuseData.BMP.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            normalBMP = Imaging.CreateBitmapSourceFromHBitmap(normalData.BMP.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        }

                        if (selectedItem.ItemName.Equals(Strings.Face))
                        {
                            normalData = TEX.GetTex(mtrlData.NormalOffset);

                            if (materialStrings[i].Contains("_fac_"))
                            {
                                specularData = TEX.GetTex(mtrlData.SpecularOffset);
                                diffuseData = TEX.GetTex(mtrlData.DiffuseOffset);

                                var maps = TexHelper.MakeCharacterMaps(normalData, diffuseData, null, specularData);

                                diffuseBMP = maps[0];
                                specularBMP = maps[1];
                                normalBMP = maps[2];
                                alphaBMP = maps[3];
                                isFace = true;

                                specularData.Dispose();
                                diffuseData.Dispose();
                            }
                            else
                            {
                                specularData = TEX.GetTex(mtrlData.SpecularOffset);
                                var maps = TexHelper.MakeCharacterMaps(normalData, diffuseData, null, specularData);

                                diffuseBMP = maps[0];
                                specularBMP = maps[1];
                                normalBMP = maps[2];
                                alphaBMP = maps[3];
                            }
                        }
                    }
                    else
                    {
                        if (mtrlData.ColorData != null)
                        {
                            var colorBmp = TEX.TextureToBitmap(mtrlData.ColorData, 9312, 4, 16);
                            colorBMP = Imaging.CreateBitmapSourceFromHBitmap(colorBmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        }

                        if (mtrlData.NormalOffset != 0)
                        {
                            normalData = TEX.GetTex(mtrlData.NormalOffset);
                        }

                        if (mtrlData.MaskOffset != 0)
                        {
                            maskData = TEX.GetTex(mtrlData.MaskOffset);
                            maskBMP = Imaging.CreateBitmapSourceFromHBitmap(maskData.BMP.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        }

                        if (mtrlData.DiffuseOffset != 0)
                        {
                            diffuseData = TEX.GetTex(mtrlData.DiffuseOffset);
                            if (mtrlData.DiffusePath.Contains("human") && !mtrlData.DiffusePath.Contains("demi"))
                            {
                                isBody = true;
                                diffuseBMP = Imaging.CreateBitmapSourceFromHBitmap(diffuseData.BMP.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                                normalBMP = Imaging.CreateBitmapSourceFromHBitmap(normalData.BMP.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            }
                        }

                        if (mtrlData.SpecularOffset != 0)
                        {
                            specularData = TEX.GetTex(mtrlData.SpecularOffset);
                            specularBMP = Imaging.CreateBitmapSourceFromHBitmap(specularData.BMP.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        }

                        if (!isBody && specularData == null)
                        {
                            var maps = TexHelper.MakeModelTextureMaps(normalData, diffuseData, maskData, null, colorBMP);
                            diffuseBMP = maps[0];
                            specularBMP = maps[1];
                            normalBMP = maps[2];
                            alphaBMP = maps[3];
                        }
                        else if (!isBody && specularData != null)
                        {
                            var maps = TexHelper.MakeModelTextureMaps(normalData, diffuseData, null, specularData, colorBMP);
                            diffuseBMP = maps[0];
                            specularBMP = maps[1];
                            normalBMP = maps[2];
                            alphaBMP = maps[3];
                        }

                        if (normalData != null)
                        {
                            normalData.Dispose();
                        }

                        if (maskData != null)
                        {
                            maskData.Dispose();
                        }

                        if (diffuseData != null)
                        {
                            diffuseData.Dispose();
                        }

                        if (specularData != null)
                        {
                            specularData.Dispose();
                        }
                    }

                    var mData = new MDLTEXData()
                    {
                        Specular = specularBMP,
                        ColorTable = colorBMP,
                        Diffuse = diffuseBMP,
                        Normal = normalBMP,
                        Alpha = alphaBMP,
                        Mask = maskBMP,

                        IsBody = isBody,
                        IsFace = isFace,

                        Mesh = meshList[i]
                    };

                    meshData.Add(mData);
                }

                CompositeVM = new Composite3DViewModel(meshData);

                is3DLoaded = true;

                ReflectionAmount = String.Format("{0:.##}", CompositeVM.SecondModelMaterial.SpecularShininess);

                try
                {

                }
                catch (Exception ex)
                {
                    MessageBox.Show("[Main] mesh 3D Error \n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                CompositeVM.Rendering(SelectedMesh.Name);
            }
        }

        /// <summary>
        /// Gets the MTRL data of the given mesh
        /// </summary>
        /// <param name="mesh">The mesh to obtain the data from</param>
        /// <returns>The MTRLData of the given mesh</returns>
        public MTRLData MTRL3D(int mesh)
        {

            MTRLData mtrlData = null;
            bool isDemiHuman = false;

            if (selectedItem.PrimaryMTRLFolder != null)
            {
                isDemiHuman = selectedItem.PrimaryMTRLFolder.Contains("demihuman");
            }
            
            var itemVersion = IMC.GetVersion(selectedCategory, selectedItem, false).Item1;
            var itemType = Helper.GetCategoryType(selectedCategory);

            try
            {
                if (selectedItem.ItemName.Equals(Strings.Face) || selectedItem.ItemName.Equals(Strings.Hair) || isDemiHuman)
                {
                    string slotAbr;

                    if (isDemiHuman)
                    {
                        slotAbr = Info.slotAbr[SelectedPart.Name];
                    }
                    else if (selectedCategory.Equals(Strings.Character))
                    {
                        var race = materialStrings[mesh].Substring(materialStrings[mesh].IndexOf("c") + 1, 4);

                        if (materialStrings[mesh].Contains("h00"))
                        {
                            var hairNum = materialStrings[mesh].Substring(materialStrings[mesh].IndexOf("h00") + 1, 4);
                            var mtrlFolder = string.Format(Strings.HairMtrlFolder, race, hairNum);
                            slotAbr = materialStrings[mesh].Substring(materialStrings[mesh].LastIndexOf("_") - 3, 3);
                            slotAbr = Info.HairTypes.FirstOrDefault(x => x.Value == slotAbr).Key;

                            var hairInfo = MTRL.GetMTRLDatafromType(selectedItem, SelectedRace, hairNum, slotAbr, itemVersion, selectedCategory);
                            return hairInfo.Item1;
                        }
                        else if (materialStrings[mesh].Contains("f00"))
                        {
                            var faceNum = materialStrings[mesh].Substring(materialStrings[mesh].IndexOf("f00") + 1, 4);
                            var mtrlFolder = string.Format(Strings.FaceMtrlFolder, race, faceNum);
                            slotAbr = materialStrings[mesh].Substring(materialStrings[mesh].LastIndexOf("_") - 3, 3);
                            slotAbr = Info.FaceTypes.FirstOrDefault(x => x.Value == slotAbr).Key;

                            var faceInfo = MTRL.GetMTRLDatafromType(selectedItem, SelectedRace, faceNum, slotAbr, itemVersion, selectedCategory);
                            return faceInfo.Item1;
                        }
                        else
                        {
                            slotAbr = selectedPart.Name;
                        }
                    }
                    else
                    {
                        slotAbr = selectedPart.Name;
                    }

                    var info = MTRL.GetMTRLDatafromType(selectedItem, SelectedRace, selectedPart.Name, slotAbr, itemVersion, selectedCategory);
                    mtrlData = info.Item1;
                }
                else
                {
                    Tuple<MTRLData, ObservableCollection<ComboBoxInfo>> info;

                    if (itemType.Equals("character") || itemType.Equals("equipment"))
                    {
                        try
                        {
                            if (materialStrings[mesh].Contains("b00") || materialStrings[mesh].Contains("t00") || materialStrings[mesh].Contains("h00"))
                            {
                                if (materialStrings[mesh].Contains("mt_c"))
                                {
                                    var mtrlFolder = "";
                                    var race = materialStrings[mesh].Substring(materialStrings[mesh].IndexOf("c") + 1, 4);

                                    if (materialStrings[mesh].Contains("b00"))
                                    {
                                        mtrlFolder = string.Format(Strings.BodyMtrlFolder, race, materialStrings[mesh].Substring(materialStrings[mesh].IndexOf("b00") + 1, 4));
                                    }
                                    else if (materialStrings[mesh].Contains("t00"))
                                    {
                                        mtrlFolder = string.Format(Strings.TailMtrlFolder, race, materialStrings[mesh].Substring(materialStrings[mesh].IndexOf("t00") + 1, 4));
                                    }
                                    else if (materialStrings[mesh].Contains("h00"))
                                    {
                                        mtrlFolder = string.Format(Strings.HairMtrlFolder, race, materialStrings[mesh].Substring(materialStrings[mesh].IndexOf("h00") + 1, 4));
                                    }

                                    var mtrlFile = materialStrings[mesh].Substring(1);

                                    return MTRL.GetMTRLInfo(Helper.GetItemOffset(FFCRC.GetHash(mtrlFolder), FFCRC.GetHash(mtrlFile)), true);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            Debug.WriteLine(ex.StackTrace);
                        }
                    }
                    else
                    {
                        info = MTRL.GetMTRLData(selectedItem, SelectedRace.ID, selectedCategory, SelectedPart.Name, itemVersion, "", "", "0000");
                    }

                    if (SelectedPart.Name.Equals("Secondary"))
                    {
                        info = MTRL.GetMTRLData(selectedItem, SelectedRace.ID, selectedCategory, SelectedPart.Name, itemVersion, "Secondary", "", "0000");
                    }
                    else
                    {
                        string part = "a";
                        string itemID = selectedItem.PrimaryModelID;

                        if (materialStrings.Count > 1)
                        {
                            try
                            {
                                part = materialStrings[mesh].Substring(materialStrings[mesh].LastIndexOf("_") + 1, 1);
                                itemID = materialStrings[mesh].Substring(materialStrings[mesh].IndexOf("_") + 2, 4);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                                Debug.WriteLine(ex.StackTrace);
                            }
                        }

                        if (selectedCategory.Equals(Strings.Pets))
                        {
                            part = "1";
                        }

                        info = MTRL.GetMTRLData(selectedItem, SelectedRace.ID, selectedCategory, part, itemVersion, "", itemID, "0000");
                    }

                    if (info != null)
                    {
                        mtrlData = info.Item1;
                    }
                    else
                    {
                        var combo = new ComboBoxInfo() { Name = "Default", ID = materialStrings[mesh].Substring(materialStrings[mesh].IndexOf("c") + 1, 4), IsNum = false };

                        if (SelectedPart.Name.Equals("-"))
                        {
                            info = MTRL.GetMTRLData(selectedItem, combo.ID, selectedCategory, "a", itemVersion, "", "", "0000");
                        }
                        else
                        {
                            info = MTRL.GetMTRLData(selectedItem, combo.ID, selectedCategory, SelectedPart.Name, itemVersion, "", "", "0000");
                        }
                       
                        mtrlData = info.Item1;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                return null;
            }

            return mtrlData;
        }



        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
