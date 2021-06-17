### THIS SCRIPT IS GOING TO ANALYZE DATA IN ORDER TO TEST HYPOTHESIS FROM 
### VARIABLE: pattern-total_time_ms

### libraries installed

# library to manage data
library(dplyr)

# library for plots
library(ggpubr)

#library for some statistical tests
library(car)


### read data
input_path = './Downloads/Proyecto - Driving stress/Data/Data_cogito/Processed_data/final_tests/2_filter2/'
input_file_pattern = 'pattern_data.csv'
in_data_pattern <- read.csv( paste(input_path,input_file_pattern, sep ='') , sep = ';' )

# Transform none to na
in_data_pattern <- na_if(in_data_pattern, 'None')

# transform to numeric values
in_data_pattern$type_test <- as.factor(in_data_pattern$type_test)
in_data_pattern[, 5]  <- as.logical(in_data_pattern[, 5])
in_data_pattern[, c(1, 3:4, 6:7)]  = as.numeric( unlist( in_data_pattern[, c(1, 3:4, 6:7)] ) )
str(in_data_pattern)


### some graphs
# to apply anova, it REQUIRES that data has a gaussian behavior 
# histogram and qqplot by levels to see normal behaviour

hist( in_data_pattern[ in_data_pattern$level==0, ]$total_time_ms )
hist( in_data_pattern[ in_data_pattern$level==1, ]$total_time_ms )
hist( in_data_pattern[ in_data_pattern$level==2, ]$total_time_ms  )
ggqqplot(  in_data_pattern[ in_data_pattern$level==0, ]$total_time_ms )
ggqqplot(  in_data_pattern[ in_data_pattern$level==1, ]$total_time_ms )
ggqqplot(  in_data_pattern[ in_data_pattern$level==2, ]$total_time_ms )


# Box plots by type of tests. Level 0
ggboxplot(in_data_pattern[ in_data_pattern$level==0, ] , x = "type_test", y = "total_time_ms", 
          color = "type_test", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue"),
          order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
          ylab = "total_time_ms", xlab = "Test type", title = "total_time_ms in LEVEL 0" ,
)

# Box plots by type of tests. Level 1
ggboxplot(in_data_pattern[ in_data_pattern$level==1, ] , x = "type_test", y = "total_time_ms", 
          color = "type_test", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue"),
          order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
          ylab = "total_time_ms", xlab = "Test type", title = "total_time_ms in LEVEL 1" ,
)

# Box plots by type of tests. Level 2
ggboxplot(in_data_pattern[ in_data_pattern$level==2, ] , x = "type_test", y = "total_time_ms", 
          color = "type_test", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue"),
          order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
          ylab = "total_time_ms", xlab = "Test type", title = "total_time_ms in LEVEL 2" ,
)


### ANALYSIS

# defining a function to compute anova with their assumptions and tukey as post-hoc test
anovaAnalysis <- function(level, varY, data){
  # testing normality
  hist( data[ (data$level==level), varY ] , 20  )
  plot(ggqqplot( data[ (data$level==level), varY ] ))
  
  # testing homoscedasticity
  res.aov <- aov( eval(as.symbol(varY)) ~ type_test , data = data[ (data$level==level),  ]  )
  plot(res.aov, 1)
  print( leveneTest( eval(as.symbol(varY)) ~ type_test , data = data[ (data$level==level),  ]  ) )
  
  # anova results
  print('Anova: ')
  print(summary(res.aov))
  
  #post-hoc Test
  print("Tukey test")
  TukeyHSD(res.aov)
}


### ONE-WAY ANOVA TEST: +info: http://www.sthda.com/english/wiki/one-way-anova-test-in-r
# Null hypothesis: the means of the different groups are the same
# Alternative hypothesis: At least one sample mean is not equal to the others.
# if p-value is small, it means there is at least one group with a mean value with a significant difference

# Here i am gonna test if there are differences in the response times according to the level
# according to the type of test for level 0
# I am expecting that all means are similar, i.e. there is no significant difference, i.e. a big p-value
# expecting. levene with big p-value
# expecting. anova  with big p-value
anovaAnalysis(level = 0, varY = "total_time_ms", data = in_data_pattern)

# according to the type of test for level 1
# I am expecting that all means are similar, i.e. there is no significant difference, i.e. a big p-value
# expecting. levene with big p-value
# expecting. anova  with big p-value
anovaAnalysis(level = 1, varY = "total_time_ms", data = in_data_pattern)

# according to the type of test for level 2
# I am expecting that all means have a significant difference, i.e. a small p-value
# expecting. levene with big p-value
# expecting. anova  with small p-value
anovaAnalysis(level = 2, varY = "total_time_ms", data = in_data_pattern)



#
#
##############################
########## LOG TRANSFORMATION
##############################

# making data to have a normal distribution
# applying LOG FUNCTION as transformation
in_data_pattern$total_time_ms <- log( in_data_pattern$total_time_ms )

# level 0
# expecting. levene with big p-value
# expecting. anova  with big p-value
anovaAnalysis(level = 0, varY = "total_time_ms", data = in_data_pattern)

# level 1
# I am expecting that all means are similar, i.e. there is no significant difference, i.e. a big p-value
# expecting. levene with big p-value
# expecting. anova  with big p-value
anovaAnalysis(level = 1, varY = "total_time_ms", data = in_data_pattern)

# level 2
# I am expecting that all means have a significant difference, i.e. a small p-value
# expecting. levene with big p-value
# expecting. anova  with small p-value
anovaAnalysis(level = 2, varY = "total_time_ms", data = in_data_pattern)

##########
# non-parametric Kruskal-wallis test
kruskal.test(total_time_ms ~ type_test , data = in_data_pattern[ in_data_pattern$level==2, ])

##########





# next code lines are replaced by code inside the function anovaanalysis()
# in level 2 make anova comparison pairwise
# Tukey multiple pairwise-comparisons. + info: http://www.sthda.com/english/wiki/one-way-anova-test-in-r
#
# tukey's assumptions:
# 1- The observations being tested are independent within and among the groups.
# 2- The groups associated with each mean in the test are normally distributed.
# 3- There is equal within-group variance across the groups associated with each mean in the test (homogeneity of variance). (Use the levenes test)
#     --> to test 3, the levenes test: 
#                   - the null hypothesis is that all populations variances are equal;
#                   - the alternative hypothesis is that at least two of them differ.
#         For levenes test, I expect that I cannot reject the null H, i.e. a big p-value
leveneTest(total_time_ms ~ type_test , data = in_data_pattern[ in_data_pattern$level==2, ])

# diff: difference between means of the two groups
# lwr, upr: the lower and the upper end point of the confidence interval at 95% (default)
# p adj: p-value after adjustment for the multiple comparisons.
# a small p-value implies that that diff is significant
TukeyHSD(res.aov)


## In some part I read TukeyTest is better than t-test. Thus, I am NOT GONNA do t-test for comparison
# t-test to see if a group has a greater mean value than the other
# 'greater' means Ha: average-group1 > average-group2
# 'less'    means Ha: average-group1 < average-group2
# versus = c(' Base', ' Auditory')
# t.test( in_data_pattern[ in_data_pattern$type_test == versus[1] & in_data_pattern$level==2, ]$total_time_ms,
#        in_data_pattern[ in_data_pattern$type_test == versus[2] & in_data_pattern$level==2, ]$total_time_ms,
#        alternative = "greater"
# )





