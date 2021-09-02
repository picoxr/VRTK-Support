# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.6.0] - 2021-06-25

Plastic SCM for Unity is now available as part of the Version Control Package! You can enable Plastic SCM via Window > Plastic SCM to get started!
If you have previously used the Unity Asset Store Plastic SCM plug-in, you can now simply use this package. Make sure you delete the plug-in from your project.
Removing a previously added Plastic SCM Asset Store Plug-In:
- Select the PlasticSCM folder in the Assets\Plugins folder on the Project tab, then click Edit > Delete
- Close the Unity Editor and open your project again. You will find the Plastic SCM menu item in the Window menu.
### Added
- Added support for inviting other members. This option is available from the gear / settings icon.
- Added support for signing in with Cloud Edition. This is available during the onboarding screen if you have never signed in.
- Added support for turning off Plastic in their project. This option removes the Plastic metadata from your directory. This option is available under Assets > Plastic SCM > Turn off Plastic SCM
- Added notification on the Plastic SCM tab title to indicate incoming changes. Users will no longer need to have the Plastic SCM window visible to know there are incoming changes.
- Auto configuration of SSO
- Added date column in incoming changes
### Changed
- Updating license to better conform with expected customer usage.
- Updated documentation file to meet standards.
- Updated third-party usage.
- No longer requires downloading of the full Plastic client. Basic features will work without additional installation. Features that require the full Plastic client will allow download and install as needed.
- Usability improvements around checking in code
- Improved update workspace tab UX
- Plastic SCM context menu is now available even if the Plastic SCM window is closed
### Fixed
- Stability and performance improvements

## [1.4.9] - 2021-06-03
### Unreleased
- The Version Control package will be expanding to include both Collaborate and Plastic SCM version control interfaces. This release is preparing for that move and contains no new functionality or bug fixes for Collaborate.
### Changed
- Collaborate Package renamed to Version Control with changes to package display name and description.
- Updating license to better conform with expected customer usage.
- Updated documentation file to meet standards.
- Updated third-party usage.

## [1.2.16] - 2019-02-11
Update stylesheet to pass USS validation

## [1.2.15] - 2018-11-16
Added support for non-experimental UIElements.

## [1.2.11] - 2018-09-04
Made some performance improvements to reduce impact on ReloadAssemblies.

## [1.2.9] - 2018-08-13
Test issues for the Collab History Window are now fixed.

## [1.2.7] - 2018-08-07
Toolbar drop-down will no longer show up when package is uninstalled.

## [1.2.6] - 2018-06-15
Fixed an issue where Collab's History window wouldn't load properly.

## [1.2.5] - 2018-05-21
This is the first release of *Unity Package CollabProxy*.

### Added
- Collab history and toolbar windows
- Collab view and presenter classes
- Collab Editor tests for view and presenter
