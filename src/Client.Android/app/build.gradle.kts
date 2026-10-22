import com.google.protobuf.gradle.*

plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
    id("org.jetbrains.kotlin.plugin.compose")
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
    }
    buildFeatures { compose = true }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }
    kotlinOptions { jvmTarget = "17" }
}
protobuf {
    protoc { artifact = "com.google.protobuf:protoc:4.36.2" }
    generateProtoTasks {
        all().configureEach {
            builtins {
                create("java") { option("lite") }
                create("kotlin") { option("lite") }
            }
        }
    }
}

dependencies {
    implementation("com.google.protobuf:protobuf-javalite:4.36.2")
    implementation("com.google.protobuf:protobuf-kotlin-lite:4.36.2")
    implementation("androidx.core:core-ktx:1.17.0")
    implementation("androidx.activity:activity-compose:1.11.0")
    implementation("androidx.compose.ui:ui:1.9.2")
    implementation("androidx.compose.ui:ui-tooling-preview:1.9.2")
    implementation("androidx.compose.material3:material3:1.4.0")
    implementation("androidx.lifecycle:lifecycle-runtime-ktx:2.9.3")
    testImplementation("junit:junit:4.13.2")
    debugImplementation("androidx.compose.ui:ui-tooling:1.9.2")
}
