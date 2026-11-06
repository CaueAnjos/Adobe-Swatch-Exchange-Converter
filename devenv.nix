{pkgs, ...}: {
  languages.dotnet = {
    enable = true;
    package = pkgs.dotnetCorePackages.sdk_10_0;
  };
}
