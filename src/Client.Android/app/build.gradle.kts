import com.google.protobuf.gradle.*

plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
    id("org.jetbrains.kotlin.plugin.compose")
    id("org.jetbrains.kotlin.kapt")
    id("com.google.protobuf")
    id("org.jlleitschuh.gradle.ktlint")
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
    buildFeatures { compose = true }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }
    kotlinOptions { jvmTarget = "17" }
    testOptions {
        unitTests.isIncludeAndroidResources = true
    }
    packaging {
        resources {
            excludes += setOf(
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
    testImplementation("junit:junit:4.13.2")
    testImplementation("org.robolectric:robolectric:4.17")
    debugImplementation("androidx.compose.ui:ui-tooling:1.9.2")
}
