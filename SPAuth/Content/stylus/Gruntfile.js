'use strict';
var LIVERELOAD_PORT = 35729;
var lrSnippet = require('connect-livereload')({ port: LIVERELOAD_PORT });
var mountFolder = function (connect, dir) {
	return connect.static(require('path').resolve(dir));
};

module.exports = function (grunt) {
	require('load-grunt-tasks')(grunt);
	require('time-grunt')(grunt);

	grunt.initConfig({
		watch: {
			stylus: {
				files: [
				'**/*.styl'
				],
				tasks: ['stylus']
			},
			livereload: {
				options: {
					livereload: LIVERELOAD_PORT
				},
				files: [
					'../css/{,*/}*.css',
					'../css/bootstrap.css',
					'../css/{,*/}*.{png,jpg,jpeg,gif,webp,svg}'
				]
			}
		},
		stylus: {
			compile: {
				options: {
					compress: true,
					paths: ['node_modules/stylus/node_modules'],
					use: [require('nib')],
					import: ['nib']
				},
				files: {
					'../css/site.css': ['site/style.styl'],
					'../css/bootstrap.css': ['bootstrap/bootstrap.styl']
				}
			}
		}
	});

	grunt.registerTask('watchStyles', function(){
		grunt.task.run(['watch']);
	});
	grunt.registerTask('compass', ['stylus']);
};