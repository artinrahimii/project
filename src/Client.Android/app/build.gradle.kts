import com.google.protobuf.gradle.*
import java.io.File
import org.gradle.api.GradleException

plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
    id("org.jetbrains.kotlin.plugin.compose")
    id("org.jetbrains.kotlin.kapt")
    id("com.google.protobuf")
    id("org.jlleitschuh.gradle.ktlint")
}

val robolectricDependencyRepoUrl =
    providers
        .gradleProperty("robolectricDependencyRepoUrl")
        .orElse(providers.environmentVariable("ROBOLECTRIC_DEPENDENCY_REPO_URL"))
        .orElse("https://maven-central.storage-download.googleapis.com/maven2/")

// Release signing material is supplied only through the runner environment or a
// protected GitHub Environment. Never put passwords or keystore files in the repository.
val releaseSigningInputs = mapOf(
    "WHITEP_ANDROID_RELEASE_STORE_FILE" to System.getenv("WHITEP_ANDROID_RELEASE_STORE_FILE"),
    "WHITEP_ANDROID_RELEASE_STORE_PASSWORD" to System.getenv("WHITEP_ANDROID_RELEASE_STORE_PASSWORD"),
    "WHITEP_ANDROID_RELEASE_KEY_ALIAS" to System.getenv("WHITEP_ANDROID_RELEASE_KEY_ALIAS"),
    "WHITEP_ANDROID_RELEASE_KEY_PASSWORD" to System.getenv("WHITEP_ANDROID_RELEASE_KEY_PASSWORD"),
)
val missingReleaseSigningInputs = releaseSigningInputs.filterValues { it.isNullOrBlank() }.keys
val releaseSigningConfigured = missingReleaseSigningInputs.isEmpty()
val requestedGradleTasks = gradle.startParameter.taskNames.map { it.substringAfterLast(':') }
val releaseBuildRequested = requestedGradleTasks.any {
    it.contains("release", ignoreCase = true) ||
        it.equals("build", ignoreCase = true) ||
        it.equals("assemble", ignoreCase = true) ||
        it.equals("bundle", ignoreCase = true)
}
if (releaseBuildRequested && !releaseSigningConfigured) {
    throw GradleException(
        "Release tasks require all four WHITEP_ANDROID_RELEASE_* environment variables. Missing: " +
            missingReleaseSigningInputs.joinToString(", "),
    )
}
val releaseSigningStoreFile = releaseSigningInputs["WHITEP_ANDROID_RELEASE_STORE_FILE"]
    ?.takeIf { it.isNotBlank() }
    ?.let { File(it).canonicalFile }
if (releaseBuildRequested) {
    val signingFile = releaseSigningStoreFile ?: throw GradleException(
        "Release signing keystore path is missing.",
    )
    if (!signingFile.isFile) {
        throw GradleException("Release signing keystore does not exist or is not a file: " + signingFile.path)
    }
    val repositoryRoot = rootProject.projectDir.resolve("../..").canonicalFile.toPath()
    if (signingFile.toPath().startsWith(repositoryRoot)) {
        throw GradleException("Release signing keystore must be stored outside the repository checkout.")
    }
}

android {
    sourceSets {
        getByName("main") {
            proto {
                srcDir("../../../protocol")
            }
        }
        getByName("debug") {
            java.srcDir(layout.buildDirectory.dir("generated/sources/proto/debug/kotlin"))
            java.srcDir(layout.buildDirectory.dir("generated/source/proto/debug/kotlin"))
        }
        getByName("release") {
            java.srcDir(layout.buildDirectory.dir("generated/sources/proto/release/kotlin"))
            java.srcDir(layout.buildDirectory.dir("generated/source/proto/release/kotlin"))
        }
    }
    namespace = "com.whitep.client"
    compileSdk = 36
    defaultConfig {
        applicationId = "com.whitep.client"
        minSdk = 26
        targetSdk = 36
        versionCode = 1
        versionName = "0.1.0"
        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
    }
    signingConfigs {
        if (releaseSigningConfigured) {
            create("release") {
                storeFile = releaseSigningStoreFile
                storePassword = releaseSigningInputs["WHITEP_ANDROID_RELEASE_STORE_PASSWORD"]
                keyAlias = releaseSigningInputs["WHITEP_ANDROID_RELEASE_KEY_ALIAS"]
                keyPassword = releaseSigningInputs["WHITEP_ANDROID_RELEASE_KEY_PASSWORD"]
            }
        }
    }
    buildTypes {
        getByName("debug") {
            enableUnitTestCoverage = true
            enableAndroidTestCoverage = true
        }
        getByName("release") {
            if (releaseSigningConfigured) {
                signingConfig = signingConfigs.getByName("release")
            }
        }
    }
    buildFeatures { compose = true }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }
    kotlinOptions { jvmTarget = "17" }
    testOptions {
        unitTests.isIncludeAndroidResources = true
        unitTests.all {
            // Robolectric fetches large instrumented Android SDK jars at test runtime.
            // Maven Central's repo1 endpoint can return HTTP 429 on shared CI runners.
            // Keep the mirror configurable for local/CI environments.
            it.systemProperty("robolectric.dependency.repo.url", robolectricDependencyRepoUrl.get())
            it.systemProperty("robolectric.dependency.repo.id", "google-maven-central")
            it.jvmArgs(
                "--add-opens=java.base/java.lang=ALL-UNNAMED",
                "--add-opens=java.base/java.util=ALL-UNNAMED",
                "--add-opens=java.base/java.io=ALL-UNNAMED",
                "--add-opens=java.base/java.net=ALL-UNNAMED",
                "--add-opens=java.base/java.security=ALL-UNNAMED",
                "--add-opens=java.base/java.text=ALL-UNNAMED",
                "--add-opens=java.base/jdk.internal.access=ALL-UNNAMED",
                "--add-opens=java.desktop/java.awt.font=ALL-UNNAMED",
                "--add-opens=jdk.compiler/com.sun.tools.javac.api=ALL-UNNAMED",
            )
        }
    }
    packaging {
        resources {
            excludes +=
                setOf(
                    "META-INF/io.netty.versions.properties",
                    "META-INF/INDEX.LIST",
                    "META-INF/versions/9/OSGI-INF/MANIFEST.MF",
                    "META-INF/native-image/io.netty/netty-codec-native-quic/jni-config.json",
                    "META-INF/native-image/io.netty/netty-codec-native-quic/reflect-config.json",
                    "META-INF/native-image/io.netty/netty-codec-native-quic/resource-config.json",
                    "META-INF/native-image/io.netty/netty-codec-native-quic/native-image.properties",
                    "META-INF/license/*",
                )
        }
    }
}
protobuf {
    protoc { artifact = "com.google.protobuf:protoc:4.36.2" }
    generateProtoTasks {
        all().configureEach {
            builtins {
                create("java")
                create("kotlin")
            }
        }
    }
}
kapt {
    arguments {
        arg("room.schemaLocation", "$projectDir/schemas")
    }
}
dependencies {
    // jvm-libp2p exposes the full Protobuf runtime. Keep one runtime on Android to avoid
    // duplicate com.google.protobuf classes from mixing the Lite and full runtimes.
    implementation("com.google.protobuf:protobuf-java:4.36.2")
    implementation("com.google.protobuf:protobuf-kotlin:4.36.2")
    implementation("io.libp2p:jvm-libp2p:1.3.7-RELEASE") {
        // This client currently dials direct TCP + Noise; native QUIC/TLS transports are not used.
        exclude(group = "io.netty", module = "netty-codec-native-quic")
        exclude(group = "io.netty", module = "netty-tcnative-boringssl-static")
    }
    implementation("androidx.core:core-ktx:1.17.0")
    implementation("androidx.activity:activity-compose:1.11.0")
    implementation("androidx.navigation:navigation-compose:2.9.8")
    implementation("androidx.compose.ui:ui:1.9.2")
    implementation("androidx.compose.ui:ui-tooling-preview:1.9.2")
    implementation("androidx.compose.material3:material3:1.4.0")
    implementation("androidx.lifecycle:lifecycle-runtime-ktx:2.9.3")
    implementation("androidx.lifecycle:lifecycle-viewmodel-compose:2.9.3")
    implementation("androidx.room:room-runtime:2.8.5")
    kapt("androidx.room:room-compiler:2.8.5")
    implementation("net.zetetic:sqlcipher-android:4.20.0")
    implementation("androidx.sqlite:sqlite:2.7.0")
    testImplementation("junit:junit:4.13.2")
    androidTestImplementation("androidx.test:core:1.7.0")
    androidTestImplementation("androidx.test:runner:1.7.0")
    androidTestImplementation("androidx.test.ext:junit:1.3.0")
    testImplementation("org.robolectric:robolectric:4.17")
    debugImplementation("androidx.compose.ui:ui-tooling:1.9.2")
}
