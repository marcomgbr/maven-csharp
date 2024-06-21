# Optimizing Software Development with Modularization and Dependency Management

By [Marco Aurélio Oliveira](https://maurelio.com.br)

One of the fundamental principles in software development is modularization: dividing the application into smaller, loosely-coupled parts. This approach facilitates maintenance, code reuse, and version management.

## Version Management and Testing
Managing versions, testing, and potential downgrades of applications are crucial. Development tools offer various options for code sharing, but version control can be challenging, requiring robust version control systems like Git and complex and exaustive schemes involving branches, tags, rebases, worktrees, and so on...

## Common Issues with Code Sharing
Code sharing has its challenges. Compiling on different machines with different configurations can produce different binaries, leading to inconsistent behaviors and complicating debugging and testing. Binary files, on the other hand, are immutable, ensuring that what was tested is what goes into production.

## Limitations of Development Tools
Tools like Visual Studio allow the creation of solutions with multiple projects and adding dependencies between them, but this increase coupling and make codebase restructuring difficult. Additionally, NuGet, while useful, is not always straightforward to implement and automate.

## Effective Solutions: Maven and Alternatives
In the Java ecosystem, Maven stands out by facilitating binary compartmentalization and reuse. Inspired by this approach, I sought a similar solution for version control in C#, which can also be adapted for other languages and IDEs.

# Getting Started with Maven for C#
Keeping this example as simple as possible, inheritance of POM files was not used.
## A Project without Dependencies - PrimaryDLL
1. Install and configure Maven on your computer.
2. In Visual Studio, open the "PrimaryDLL" solution, right-click on the project node, and select `Build`;
>*A "Debug" version will be generated in PROJECT_HOME\bin\Debug. If you select "Release" mode, a version will be generated in PROJECT_HOME\bin\Release.*
3. If you generated a "Debug" version, run `mvn install -P debug` in PROJECT_HOME. If you generated a "Release" version, simply run `mvn install`, without the need for the release parameter (profile, in fact);
>*The files will be packaged into a JAR file and copied to the local Maven repository, usually USER_HOME\\.m2\repository.*

## A Project with Dependencies - DependentDLL
1. Ensure all dependencies are installed in the local or remote Maven repository;
>*In the case of the "DependentDLL" project, the only dependency is "PrimaryDLL.dll" - which was installed in the previous process in this tutorial - as specified in the POM file:*
```
<dependencies>
    <dependency>
        <groupId>maurelio-com-br</groupId>
        <artifactId>primary-dll</artifactId>
        <version>1.0</version>
    </dependency>
</dependencies>
```

2. In the root folder of the "DependentDLL" project, run `mvn validate`;
>*The "PrimaryDLL.dll" library, which is in the repository as a JAR file, will be copied and extracted to the PROJECT_HOME\lib folder.*
3. In Visual Studio, add the reference to "PrimaryDLL.dll" to the project;
>*In this tutorial, the reference has already been added.*
4. In Visual Studio, execute "Build";
5. If you generated a "Debug" version, run `mvn install -P debug` in PROJECT_HOME. If you generated a "Release" version, simply run `mvn install`;
>*A JAR file containing "PrimaryDLL.dll" and "DependentDLL.dll" will be generated, and this file will be copied to the local Maven repository.*

## ConsumerApplication Project
To compile this application, simply follow steps 1 to 4 of DependentDLL. 
>*"PrimaryDLL.dll" and "DependentDLL.dll" will be added to project "lib" folder.*  
>*In principle, there is no need to execute step 5, `mvn install`, as this is an application, not a library.*

## Running Maven from Visual Studio
During the project phase, it may be useful to automatically execute some phases of the Maven project when "Build" is executed in Visual Studio.
To do this, just include tasks for automatic execution in the ".csproj" project file, as shown in the following example:

### In the DLL project file, add:
```
<Target Name="InstallPackageDebug" AfterTargets="PostBuildEvent" Condition="'$(Configuration)' == 'Debug'">
    <Exec Command="mvn install -P debug" />
</Target>
<Target Name="InstallPackageRelease" AfterTargets="PostBuildEvent" Condition="'$(Configuration)' == 'Release'">
    <Exec Command="mvn install" />
</Target>
```

### In the project file that consumes the DLL, add:
```
<Target Name="CopyDependenciesDebug" BeforeTargets="PreBuildEvent"  Condition="'$(Configuration)' == 'Debug'">
    <Exec Command="mvn validate -P debug" />
</Target>
<Target Name="CopyDependenciesRelease" BeforeTargets="PreBuildEvent"  Condition="'$(Configuration)' == 'Release'">
    <Exec Command="mvn validate" />
</Target>
```

Refer to [Tutorial: Use MSBuild](https://learn.microsoft.com/en-us/visualstudio/msbuild/walkthrough-using-msbuild?view=vs-2022) from Microsoft.

This way, developers can switch between the tasks of modifying the DLL and testing it wthout having to type Maven commands all the time.  
In the case of development on more than one computer, it is necessary to create a file server (remote repository, which can be private) to share the files with other developers.

## Creating a Private Repository for Your Team
A simple and inexpensive way to experiment with this development process is to turn a computer on your internal network into a file server (remote repository). It can even be a user's PC temporarily.

The repository can be implemented via SCP, SFTP, FTP, WebDAV, or by filesystem, with a shared folder, for example.
See [Introduction to Repositories](https://maven.apache.org/guides/introduction/introduction-to-repositories.html).

I chose an FTP server - Filezilla - which takes 2 minutes and 37 seconds to install and configure.

After creating the FTP server, or hiring a server in the cloud, just change the "MAVEN_HOME\conf\settings.xml" file on the client machine and insert the following item in the `<servers>` section:

```
<settings>
    ...
    <servers>
        ...
        <!-- You can have many servers, or many repositories on each server -->
        <server>
          <id>your-repository-id</id> <!-- You select the repository by id -->
          <username>your-username</username>
          <password>your-password</password>
        </server>
        ...
    </servers>
    ...
</settings>
```

In your POM file, include the following item:
```
<project>
  ...
  <distributionManagement>
        <repository>
            <id>your-repository-id</id> <!-- You select the repository by id -->
            <url>ftp://some-ip-or-url/your_root_folder</url> <!-- default port can be ommited -->
        </repository>
        <snapshotRepository>
            <id>your-repository-id</id>
            <url>ftp://some-ip-or-url/your_root_folder</url>
        </snapshotRepository>
    </distributionManagement>
  ...
</project>
```

After that, developers can execute `mvn deploy` to make their libraries available to other developers/testers/reviewers/configuration managers.

## Final Considerations
- **Tip:** Maven's automatic execution can be manually adjusted on demand, saving a few seconds of processing time when immediate data sharing with other developers is not necessary. It's all about adjusting workflows to fit your needs.
- During tests, the `mvn clean validate -P [release/debug]` command prevents JAR files with "junk" possibly generated in previous executions from being generated.
- A good coding standard replaces a lot of unnecessary documentation and facilitates collaboration and maintenance. Maven archetypes aim to achieve this.