static BuildArguments args;
args = new BuildArguments {
  Target = Argument("target", Argument("t", "build")),
  Configuration = Argument("configuration", Argument("c", "Debug"))
};

class BuildArguments {
  public string Target { get; init; }
  public string Configuration { get; init; }
}
