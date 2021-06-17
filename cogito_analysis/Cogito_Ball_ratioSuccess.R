### THIS SCRIPT IS GOING TO ANALYZE DATA IN ORDER TO TEST HYPOTHESIS FROM 
### VARIABLE: a new variable is created: ratio success= num_perfect_balls / total_balls

### libraries installed

# library to manage data
library(dplyr)

# library for plots
library(ggpubr)

#library for some statistical tests
library(car)


### READ DATA AND STRUCTURING DATA FOR ANALYSIS
input_path = './Downloads/Proyecto - Driving stress/Data/Data_cogito/Processed_data/final_tests/2_filter2/'

# LEVEL 0
input_file_pattern = 'users_score_lv0.csv'
in_data_ball_lv0 <- read.csv( paste(input_path,input_file_pattern, sep ='') , sep = ';' )
# Transform none to na
in_data_ball_lv0 <- na_if(in_data_ball_lv0, 'None')
# transform to numeric values
in_data_ball_lv0[, c(1, 3:10)]  = as.numeric( unlist( in_data_ball_lv0[, c(1, 3:10)] ) )
#str(in_data_ball_lv0)

# LEVEL 1
input_file_pattern = 'users_score_lv1.csv'
in_data_ball_lv1 <- read.csv( paste(input_path,input_file_pattern, sep ='') , sep = ';' )
# Transform none to na
in_data_ball_lv1 <- na_if(in_data_ball_lv1, 'None')
# transform to numeric values
in_data_ball_lv1[, c(1, 3:10)]  = as.numeric( unlist( in_data_ball_lv1[, c(1, 3:10)] ) )

# LEVEL 2
input_file_pattern = 'users_score_lv2.csv'
in_data_ball_lv2 <- read.csv( paste(input_path,input_file_pattern, sep ='') , sep = ';' )
# Transform none to na
in_data_ball_lv2 <- na_if(in_data_ball_lv2, 'None')
# transform to numeric values
in_data_ball_lv2[, c(1, 3:10)]  = as.numeric( unlist( in_data_ball_lv2[, c(1, 3:10)] ) )

# 
ratio_success_lvl0 <- in_data_ball_lv0$num_perfect_balls / in_data_ball_lv0$total_balls
ratio_success_lvl1 <- in_data_ball_lv1$num_perfect_balls / in_data_ball_lv1$total_balls
ratio_success_lvl2 <- in_data_ball_lv2$num_perfect_balls / in_data_ball_lv2$total_balls

in_data_ball <- data.frame( 'id_user'   = in_data_ball_lv0$id_user , 
                               'type_test' = in_data_ball_lv0$type_test , 
                               'success_lv0' = ratio_success_lvl0, 
                               'success_lv1' = ratio_success_lvl1, 
                               'success_lv2' = ratio_success_lvl2)


nData = length(ratio_success_lvl0)
in_data_ball <- data.frame( 'id_user'   = rep( in_data_ball_lv0$id_user   , 3 ) , 
                               'type_test' = rep( in_data_ball_lv0$type_test , 3 ) , 
                               'level'     = c( rep(0,nData) , rep(1,nData), rep(2,nData)  ),
                               'ratioSuccess' = c( ratio_success_lvl0 , ratio_success_lvl1 , ratio_success_lvl2 )
)

in_data_ball$type_test <- as.factor(in_data_ball$type_test)
str(in_data_ball)
rm(in_data_ball_lv0, in_data_ball_lv1, in_data_ball_lv2)

### SOME GRAPHS

# qqplot by levels to see normal behaviour
hist( in_data_ball[ in_data_ball$level == 0,  ]$ratioSucces, 20 )
hist( in_data_ball[ in_data_ball$level == 1,  ]$ratioSucces, 20 )
hist( in_data_ball[ in_data_ball$level == 2,  ]$ratioSucces, 20 )

ggqqplot( in_data_ball[ in_data_ball$level == 0,  ]$ratioSucces )
ggqqplot( in_data_ball[ in_data_ball$level == 1,  ]$ratioSucces )
ggqqplot( in_data_ball[ in_data_ball$level == 2,  ]$ratioSucces )


# Box plots by type of tests. Level 0
level = 0
ggboxplot(  in_data_ball[ in_data_ball$level == level,  ] , x = "type_test", y = "ratioSuccess", 
            color = "type_test", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue"),
            order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
            ylab = "success_lv0", xlab = "Test type", title = "success_lv0 in LEVEL 0" ,
)

# Box plots by type of tests. Level 1
level = 1
ggboxplot(  in_data_ball[ in_data_ball$level == level,  ] , x = "type_test", y = "ratioSuccess", 
            color = "type_test", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue"),
            order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
            ylab = "success_lv0", xlab = "Test type", title = "success_lv0 in LEVEL 1" ,
)

# Box plots by type of tests. Level 2
level = 2
ggboxplot(  in_data_ball[ in_data_ball$level == level,  ] , x = "type_test", y = "ratioSuccess", 
            color = "type_test", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue"),
            order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
            ylab = "success_lv0", xlab = "Test type", title = "success_lv0 in LEVEL 2" ,
)

### ANALYISIS


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

# Here i am gonna test if there are differences in the ball success (succes_lvX) according to the level

# according to the type of test for level 0
# I am expecting that all means are similar, i.e. there is no significant difference, i.e. a big p-value
# expecting. levene with big p-value
# expecting. anova  with big p-value
anovaAnalysis(level = 0, varY = "ratioSuccess", data = in_data_ball)

# according to the type of test for level 1
# I am expecting that all means are similar, i.e. there is no significant difference, i.e. a big p-value
# expecting. levene with big p-value
# expecting. anova  with big p-value
anovaAnalysis(level = 1, varY = "ratioSuccess", data = in_data_ball)

# according to the type of test for level 2
# I am expecting that all means are similar, i.e. there is no significant difference, i.e. a small p-value
# expecting. levene with big p-value
# expecting. anova  with small p-value
anovaAnalysis(level = 2, varY = "ratioSuccess", data = in_data_ball)


### ONE OPTION AS CONCLUSSION
# including the hardest level, all data are similar for all groups (big p-values), 
# it implies that the stimuli did not influence the performance in the pattern score. 
# It is a good news because the stimuli contains information related to the ball task.
# Thus, the stimuli does not affect the performance in paralel tasks!!!





















