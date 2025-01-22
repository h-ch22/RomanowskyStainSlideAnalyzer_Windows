<img src="./Docs/Images/Mockup_RomanowskyStainSlideAnalyzer.png"/><br>
<img src="./Docs/Images/ic_main.png" width="150px"/><br>

# Romanowsky Stain Slide Analyzer for Windows<br>
> Segmentation & Labeling Tool for Data Learning and Verification Exclusive to Romanowsky Stain Slide Analyzer<br>
ⓒ 2024-2025 Changjin Ha. All Rights Reserved.<br>
---
## Segmentation<br>
<img src="./Docs/Images/img_home_1.png"><br>
> Just load the images, name the file, and segmentation is complete.<br>

<img src="./Docs/Images/img_home_2.png"><br>
> You can select GPU to segment.<br>

<img src="./Docs/Images/img_home_3.png"><br>
> Also, you can use GPU Parallel for use multiple GPUs.<br>

<img src="./Docs/Images/img_home_4.png"><br>
> And you can choose model to use.<br>

### Customize Parameters<br>
<img src="./Docs/Images/img_customize_Parameters.png"><br>
> If you proceed with Automatic Segmentation, you can also modify SAM parameters.<br>

<img src="./Docs/Images/img_customize_Parameters_2.png"><br>
> You can also save presets and use them again later.<br>

### Select Points<br>
<img src="./Docs/Images/img_selectPoints.png"><br>
> Segment only the parts you want accurately through Manual Segmentation.<br>

### Segmentation Result<br>
<img src="./Docs/Images/img_segmentationResult.png"><br>
> Just wait a moment and the segmentation results will be right before your eyes.<br>

## Labeling<br>
<img src="./Docs/Images/img_labeling.png"><br>
> If you selected the Extract Bounding Boxes or Extract Masks option, try labeling using the Labeling function.<br>

### Export<br>
<img src="./Docs/Images/img_exportLabels.png"><br>
> From labeling to exporting, all in one go.<br>

## History<br>
<img src="./Docs/Images/img_history.png"><br>
> A strong assistant that not only checks and saves past results, but also replaces and exports labeling data, and analyzes and saves bounding boxes.<br>

## Image Viewer<br>
<img src="./Docs/Images/img_imageViewer.png"><br>
> A simple yet powerful tool to view, zoom, rotate and save images.<br>

## Analyze<br>
<img src="./Docs/Images/img_analyze.png"><br>
> Once labeling is complete, try using the Analyze function, which analyzes ARGB, Hue, Saturation, and Brightness by class.<br>

### Export<br>
<img src="./Docs/Images/img_analyze_export.png"><br>
> Of course, the Export function is built in as standard.<br>

## Settings<br>
<img src="./Docs/Images/img_settings.png"><br>
> A smart friend that checks everything from history initialization to plugin version management and even environment status.<br>

## Configure Environment<br>
<img src="./Docs/Images/img_environmentConfigure.png"><br>
> A solid feature that handles all environments at once, from checking WSL status to installation, Linux installation, SAM download, CUDA and cuDNN, Python installation, and even copying entry points.<br>

## Feature Activator<br>
<img src="./Docs/Images/img_featureActivator.png"><br>
> Are you having trouble installing Windows Additional Features and Linux? Feature Activator takes care of it all.<br>

## Compatibility<br>
> Romanowsky Stain Slide Analyzer for Windows is compatible with these devices.<br>

||Minimum Requirements|Recommended Requirements|
|-----|-----|-----|
|CPU|7th Gen. Intel Core i3|9th Gen. Intel Core i7|
|RAM|8GB|16GB|
|GPU|None|NVIDIA Geforce RTX 3060|
|Operating System|Windows 10 (x64, 22H2)|Windows 11|
|Storage|4GB HDD|8GB SSD|

## Release Note<br>
> Here are the release notes for each version of Romanowsky Stain Slide Analyzer for Windows.<br>

- ### Romanowsky Stain Slide Analyzer 1.4.0.0 Release Note
> #### Framework<br>
> - Saving a single file now displays the File Save Dialog instead of the Folder Picker, allowing you to customize the file's name and extension.<br>

> #### Segmentation<br>
> - Newly designed HomeView.<br>
> - You can now add files by dragging.<br>
> - You can now segment multiple images.<br>
> - You can now choose which GPU to use for inference.<br>
> - Parallel GPU option is now available.<br>
> - Shortcuts have been applied to some functions.<br>
> - The ability to cancel segmentation is now available.<br>
> - You can now apply the currently set options to all files.<br>
> - Model Selection is now available.<br>

> #### Customize Parameters<br>
> - Parameter Preset is now available.<br>
> - Number Box is now available.<br>

> #### History<br>
> - The History list is now displayed by file name.<br>
> - Shortcuts have been applied to some functions and menus.<br>
> - Image Viewer is now available.<br>
> - Calendar Date Picker is now available.<br>

> #### Labeling<br>
> - Now you can check the mask and bounding box at the same time.<br>
> - Shortcuts have been applied to some functions.<br>
> - Fixed the issue where Bounding Box indexes were not displayed completely.<br>

> #### Analyze<br>
> - You can now check mask data and bounding box data at the same time.<br>
> - Shortcuts have been applied to some functions.<br>

> #### Settings<br>
> - You can now delete the Preset.<br>

- ### Romanowsky Stain Slide Analyzer 1.3.0.0 Release Note
> #### Segmentation<br>
> - Extract Masks is now available.<br>
> - An option is available to save the image immediately after segmentation is complete.<br>

> #### History<br>
> - A completely new design for History View is now available.<br>
> - Re-labeling is now available.<br>
> - Save Mask Labeling Data is now available.<br>
> - Label Mask Data is now available.<br>
> - Save the masked image is available.<br>

> #### Labeling<br>
> - Toggle Bounding Box / Mask option is now available.<br>
> - Now when you click the back and next buttons, if there is any saved data it will be displayed.<br>

> #### Analyze<br>
> - The ability to analyze segmentation mask data is now available.<br>
> - Toggle Bounding Box / Mask option is now available.<br>
> - Fixed the issue where the average A value was not displayed properly when exporting data.<br>

- ### Romanowsky Stain Slide Analyzer 1.2.1.0 Release Note
> #### Feature Activator<br>
> - Fixed an issue where environment configuration was not performed properly on Windows 10.<br>
> #### Framework<br>
> - Improved environment configuration and segmentation to allow for drives other than C drive.<br>

- ### Romanowsky Stain Slide Analyzer 1.2.0.0 Release Note
> #### Select Points<br>
> - Fixed an issue where points were not displayed properly in Manually Segmentation mode<br>
> #### History<br>
> - Labeling Data Change is now available.<br>
> - Analyze Bounding Box is now available.<br>
> - Export Image with Bounding Boxes is now available.<br>
> #### Labeling<br>
> - Toggle Bounding Box Show/Hide is now available.<br>
> - Fixed an issue where blank spaces were appearing at the beginning of each piece of data.<br>
> - Automatic Zoom & Scroll is now available.<br>
> - Fixed an issue where labeling would be reset even if the Cancel button was pressed in the dialog that appears when there are already labeled files.<br>
> #### Analyze<br>
> - The Analyze feature is now available to view and export Color, Hue, Saturation, Brightness by Bounding Box and the overall Color, Hue, Saturation, Brightness average.<br>

- ### Romanowsky Stain Slide Analyzer 1.1.0.0 Release Note
> #### Segmentation<br>
> - Manually Segmentation is now available.<br>
> - Extract Bounding Boxes option is now available for extracting bounding box coordinates.<br>
> - Labeling is now available.<br>
> #### Select Points<br>
> - You can segment only that part by clicking on the part you want to segment with the mouse.<br>
> - You can specify the class of the area to be segmented.<br>
> #### Labeling<br>
> - You can specify a class for each bounding box area.<br>
> - You can export the labeled file to a csv file.<br>
> #### History<br>
> - You can save the result image file.<br>
> - You can re-save the labeled csv file.<br>
> - Fixed an issue where records would not display properly when the date was changed.<br>
> - Records are now displayed in most recent order.<br>
> #### Settings<br>
> - You can check and update each library version.<br>

- ### Romanowsky Stain Slide Analyzer 1.0.0.0 Release Note
> #### Segmentation<br>
> - Automatic Segmentation is now available.<br>
> - Customize Parameters is now available.<br>
> #### History<br>
> - History is now available.<br>
> #### Settings<br>
> - Check Environment configure status is now available.<br>
> #### Environment<br>
> - Romanowsky Stain Slide Analyzer Feature Activator is now available.<br>
> - Automatic configure environment is now available.<br>
