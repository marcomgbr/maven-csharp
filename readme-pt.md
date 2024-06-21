# Otimizando o Desenvolvimento de Software com Boas Práticas de Modularização e Gerenciamento de Dependências

By [Marco Aurélio Oliveira](https://maurelio.com.br)

Um dos princípios fundamentais no desenvolvimento de software é a modularização: dividir a aplicação em partes menores e menos acopladas. Isso facilita a manutenção, o reaproveitamento de código e a gestão de versões.

## Gerenciamento de Versões e Facilidades de Testes
Gerenciar versões, testes, homologação e possíveis downgrades de aplicações é crucial. Ferramentas de desenvolvimento oferecem várias opções para compartilhamento de código, mas controlar versões pode ser desafiador, exigindo sistemas de controle de versão robustos como Git e esquemas complexos de branches, tags, rebases, worktrees etc.

## Problemas Comuns no Compartilhamento de Código
Compartilhar código tem seus desafios. Compilar em diferentes máquinas com diferentes configurações pode gerar binários diferentes, resultando em comportamentos inconsistentes, dificultando o debug e os testes. Arquivos binários, por outro lado, são imutáveis, garantindo que o que foi testado é o que será colocado em produção.

## Limitações das Ferramentas de Desenvolvimento
Ferramentas como o Visual Studio permitem criar soluções com múltiplos projetos e adicionar dependências, mas isso pode aumentar o acoplamento e dificultar a reestruturação do código. Além disso, o NuGet, embora útil, não é sempre simples de implementar e automatizar.

## Soluções Eficazes: Maven e Alternativas
No ecossistema Java, o Maven se destaca ao facilitar a compartimentalização de binários e sua reutilização. Inspirado por essa abordagem, procurei uma solução semelhante para o controle de versões em C#, que também pode ser adaptada para outras linguagens e IDEs.

# Iniciando com Maven para C#
Para manter este exemplo o mais simples possível, não foi utilizada herança nos arquivos POM.
## Projeto sem Dependências - PrimaryDLL
1. Instale  e configure o Maven em seu computador;
2. No Visual Studio, abra a solução "PrimaryDLL", clique com o botão direito no nó do projeto e selecione "Build";
>*Será gerada uma versão "Debug" em PROJECT_HOME\bin\Debug. Se você selecionar o modo "Release", será gerada uma versão em PROJECT_HOME\bin\Release.*
3. Se você gerou uma versão "Debug", execute `mvn install -P debug` em PROJECT_HOME. Se você gerou uma versão "Release", basta executar `mvn install`, sem a necessidade do parâmetro (profile) `release`;
>*Os arquivos serão empacotados em um arquivo JAR e copiados para o repositório local do Maven, usualmente USER_HOME\\.m2\repository.*

## Projeto com Dependências - DependentDLL
1. Certifique-se de que todas as dependências estejam instaladas no repositório Maven local ou remoto;
>*No caso do projeto "DependentDLL.csproj", a única dependência é a "PrimaryDLL.dll" - que foi instalada no processo anterior - conforme o arquivo POM:*
```
<dependencies>
    <dependency>
        <groupId>maurelio-com-br</groupId>
        <artifactId>primary-dll</artifactId>
        <version>1.0</version>
    </dependency>
</dependencies>
```

2. Na pasta raiz do projeto "DependentDLL.csproj", execute `mvn validate`;
>*A biblioteca "PrimaryDLL.dll", que está no repositório em um arquivo JAR, será copiada e extraída para a pasta PROJECT_HOME\lib.*
3. No Visual Studio, adicione a referência a "PrimaryDLL.dll" ao projeto;
>*No caso deste exemplo, a referência já foi previamente adicionada.*
4. No Visual Studio, execute "Build";
5. Se você gerou uma versão "Debug", execute `mvn install -P debug` em PROJECT_HOME. Se você gerou uma versão "Release", basta executar `mvn install`;
>*Foi gerado um arquivo JAR contendo "PrimaryDLL.dll" e "DependentDLL.dll", e este arquivo foi instalado no repositório local do Maven.*

## Projeto ConsumerApplication
Para compilar esta aplicação, basta seguir os passos de 1 a 4 de DependentDLL. "PrimaryDLL.dll" e "DependentDLL.dll" serão copiadas para pasta "lib" do projeto.  
>*Em princípio, não faz sentido executar o passo 5, `mvn install`, uma vez que se trata de um aplicativo, e não de uma biblioteca.*

## Executando o Maven a partir do Visual Studio
Durante a fase de projeto, pode ser interessante executar automaticamente algumas fases do projeto Maven quando "Build" é executado no Visual Studio.
Para tanto, basta incluir no arquivo de projeto ".csproj" tarefas para execução automática, conforme o exemplo a seguir:
### No arquivo de projeto da DLL
```
<Target Name="InstallPackageDebug" AfterTargets="PostBuildEvent" Condition="'$(Configuration)' == 'Debug'">
    <Exec Command="mvn install -P debug" />
</Target>
<Target Name="InstallPackageRelease" AfterTargets="PostBuildEvent" Condition="'$(Configuration)' == 'Release'">
    <Exec Command="mvn install" />
</Target>
```

### No arquivo de projeto que consome a DLL
```
<Target Name="CopyDependenciesDebug" BeforeTargets="PreBuildEvent"  Condition="'$(Configuration)' == 'Debug'">
    <Exec Command="mvn validate -P debug" />
</Target>
<Target Name="CopyDependenciesRelease" BeforeTargets="PreBuildEvent"  Condition="'$(Configuration)' == 'Release'">
    <Exec Command="mvn validate" />
</Target>
```

Consulte o [Tutorial: Use MSBuild](https://learn.microsoft.com/en-us/visualstudio/msbuild/walkthrough-using-msbuild?view=vs-2022) da Mirosoft.

Dessa forma, o desenvolvedor(es) alterna(m) entre a tarefa de alterar a DLL e testá-la. A única diferença é que, no caso
de desenvolvimento em mais de um computador, é necessário instalar as DLLs em um repositório remoto (que pode ser privado) com o comando `mvn deploy`. 

## Criando um repositório privado para sua equipe
Uma forma simples e barata de experimentar esse processo de desenvolvimento é transformar um computador de sua rede interna 
em um servidor de arquivos (repositório Maven). Pode ser até mesmo o PC de algum usuário, temporariamente.

O repositório pode ser implementado via SCP, SFTP, FTP, WebDAV, ou pelo filesystem, com uma pasta compartilhada, por exemplo.
Veja [Introduction to Repositories](https://maven.apache.org/guides/introduction/introduction-to-repositories.html).

Eu optei por um servidor FTP - o Filezilla - que leva 2 minutos e 37 segundos para instalar e configurar.

Após criar o servidor FTP, ou contratar um servidor na nuvem, basta alterar, na máquina cliente, o arquivo "MAVEN_HOME\conf\settings.xml" e inserir, na seção `<servers>`, o seguinte item: 
```
<settings>
    ...
    <servers>
        ...
        <server>
          <id>your-repository-id</id>
          <username>your_username</username>
          <password>your_password</password>
        </server>
        ...
    </servers>
    ...
</settings>
```

Em seu arquivo POM, incluir o seguinte item:
```
<project>
  ...
  <distributionManagement>
        <repository>
            <id>your-repository-id</id>
            <url>ftp://192.168.1.13/your_root_folder</url>
        </repository>
        <snapshotRepository>
            <id>your-repository-id</id>
            <url>ftp://192.168.1.9/your_root_folder</url>
        </snapshotRepository>
    </distributionManagement>
  ...
</project>
```

Após isso, basta ao desenvolvedor executar `mvn deploy` para disponibilizar suas bibliotecas, em formato binário, aos demais desenvolvedores/testadores/homologadores/gerentes de configuração.

## Considerações finais
- **Dica:** A execução automática do Maven pode ser modificada manualmente, sob demanda, economizando alguns segundos de processamento quando não for necessário compartilhar dados com outros desenvolvedores imediatamente. Tudo é uma questão de ajuste dos métodos de trabalho.
- Durante os testes, o comando `mvn clean validate -P [release/debug]` evita que sejam gerados arquivos JAR com "lixo" eventualmente gerado nas execuções anteriores.
- **Padrões de Código:** Um bom padrão de código substitui muita documentação desnecessária e facilita a colaboração e manutenção. Os arquétipos Maven têm isso como objetivo.

